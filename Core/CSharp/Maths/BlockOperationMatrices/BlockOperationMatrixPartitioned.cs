using System.IO;
using System;
using Core.Threading;
using Core.FileSystem;
using InfernoDispatcher.Tasks;
using Core.Cleanup;
using InfernoDispatcher.Locking;
using Core.Pool;
using Core.NativeExtensions;
using static ManagedCuda.NPP.NPPNativeMethods.NPPi;
using Core.Maths.Vectors;


namespace Core.Maths.BlockOperationMatrices
{
    public class BlockOperationMatrixPartitioned : BlockOperationMatrix
    {
        public BlockOperationMatrix ChildTopLeftA { get; }
        public BlockOperationMatrix ChildTopRightB { get; }
        public BlockOperationMatrix ChildBottomLeftC { get; }
        public BlockOperationMatrix ChildBottomRightD { get; }
        public BlockOperationMatrixPartitioned(
            BlockOperationMatrix childTopLeftA,
            BlockOperationMatrix childTopRightB,
            BlockOperationMatrix childBottomLeftC,
            BlockOperationMatrix childBottomRightD,
            WorkingDirectoryManager workingDirectoryManager) : base(childTopLeftA.NRows + childBottomLeftC.NRows,
                childTopLeftA.NColumns + childTopRightB.NColumns,
                workingDirectoryManager)
        {
            FilePath = workingDirectoryManager.NewBinFile();
            Partitioned = true;
            ChildTopLeftA = childTopLeftA;
            ChildTopRightB = childTopRightB;
            ChildBottomLeftC = childBottomLeftC;
            ChildBottomRightD = childBottomRightD;
            if (childTopLeftA.NColumns != childBottomLeftC.NColumns) throw new Exception("Mismatch");
            if (childTopRightB.NColumns != childBottomRightD.NColumns) throw new Exception("Mismatch");
            if (childTopLeftA.NRows != childTopRightB.NRows) throw new Exception("Mismatch");
            if (childBottomLeftC.NRows != childBottomRightD.NRows) throw new Exception("Mismatch");
        }
        public override InfernoTaskWithResultBase<BlockOperationMatrix> InvertOnNewThread(
            CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore memoryAllocationSemaphore,
            GPUMathsParameters? gpuMathsParameters,
            MathsRunningMode runningMode,
            CompositeProgressHandler? parentProgressHandler,
            bool cache)
        {
            return cache
                ? InvertOnNewThread_LockForCache(cleanupHandler, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, parentProgressHandler, cache)
                : InvertOnNewThread_NoLockForCache(cleanupHandler, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, parentProgressHandler, cache);
        }

        public InfernoTaskWithResultBase<BlockOperationMatrix> InvertOnNewThread_LockForCache(
            CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore memoryAllocationSemaphore,
            GPUMathsParameters? gpuMathsParameters,
            MathsRunningMode runningMode,
            CompositeProgressHandler? parentProgressHandler,
            bool cache)
        {
            lock (_LockObjectInternalTask)
            {
                if (_InvertTask != null)
                {
                    return _InvertTask;
                }
                var inverseTask = InvertOnNewThread_NoLockForCache(cleanupHandler, memoryAllocationSemaphore,
                    gpuMathsParameters, runningMode, parentProgressHandler, cache);
                if (cache)
                {
                    _InvertTask = inverseTask;
                }
                return inverseTask;
            }
        }
        public InfernoTaskWithResultBase<BlockOperationMatrix> InvertOnNewThread_NoLockForCache(
            CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore memoryAllocationSemaphore,
            GPUMathsParameters? gpuMathsParameters,
            MathsRunningMode runningMode,
            CompositeProgressHandler? parentProgressHandler,
            bool cache)
        {

            CompositeProgressHandler? progressHandler = null;
            if (parentProgressHandler != null)
            {
                progressHandler = new CompositeProgressHandler(8);
                parentProgressHandler.AddChild(progressHandler);
            }
            CleanupHandler cleanupHandlerInternal = new CleanupHandler();
            cleanupHandlerCaller?.Add(cleanupHandlerInternal);
            // Step 1: Compute A^-1
            var aInverseTask = ChildTopLeftA.InvertOnNewThread(
                cleanupHandlerInternal, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, progressHandler, true);

            // Step 2: Compute C * A^-1 (using MultiplyOnNewThread)
            var cAInverseTask = aInverseTask.ThenCreateTask(aInverse =>
            {
                return ChildBottomLeftC.MultiplyOnNewThread(aInverse, cleanupHandlerInternal,
                    memoryAllocationSemaphore, gpuMathsParameters, runningMode,
                    progressHandler);
            });

            // Step 3: Compute A^-1 * B (using MultiplyOnNewThread)
            var aInverseBTask = aInverseTask.ThenCreateTask(aInverse =>
            {
                return aInverse.MultiplyOnNewThread(ChildTopRightB, cleanupHandlerInternal, memoryAllocationSemaphore,
                    gpuMathsParameters, runningMode,
                    progressHandler);
            });

            // Step 4: Compute the Schur complement: S = D - (C * A^-1 * B) (using MultiplyOnNewThread)
            var shurComplementTask = cAInverseTask.ThenCreateTask(CAInverse =>
            {
                return CAInverse.MultiplyOnNewThread(ChildTopRightB, cleanupHandlerInternal, memoryAllocationSemaphore,
                    gpuMathsParameters, runningMode,
                    progressHandler)
                .ThenCreateTask(mult => ChildBottomRightD.SubtractOnNewThread(
                    mult, cleanupHandlerInternal, memoryAllocationSemaphore));
            });

            // Step 5: Invert the Schur complement: S^-1
            var shurComplementInverseTask = shurComplementTask.ThenCreateTask(shurComplement =>
            {
                return shurComplement.InvertOnNewThread(null, memoryAllocationSemaphore,
                    gpuMathsParameters, runningMode,
                    progressHandler, true);
            });

            // Step 6: Compute A^-1 * B * S^-1 (using MultiplyOnNewThread)
            var AInverseBSInverseTask = shurComplementInverseTask.Join(aInverseBTask, (shurComplementInverse, aInverseB) =>
            {
                return (aInverseB, shurComplementInverse);
            }).ThenCreateTask(args =>
            {
                var (aInverseB, shurComplementInverse) = args;
                return aInverseB.MultiplyOnNewThread(shurComplementInverse, cleanupHandlerInternal,
                    memoryAllocationSemaphore, gpuMathsParameters, runningMode,
                    progressHandler);
            }); ;

            // Step 7: Compute new top-right: -(A^-1 * B * S^-1)
            var newTopRightTask = AInverseBSInverseTask.ThenCreateTask(aInverseBSInverse =>
            {
                return aInverseBSInverse.ScaleOnNewThread(-1, null, memoryAllocationSemaphore);
            });

            // Step 8: Compute new top-left: A^-1 + (A^-1 * B * S^-1 * C * A^-1) (using MultiplyOnNewThread)
            var aInverseBInverseCAInverseTask = AInverseBSInverseTask.Join(cAInverseTask, (aInverseBSInverse, cAInverse) =>
            {
                return (aInverseBSInverse, cAInverse);
            }).ThenCreateTask(args =>
            {
                var (aInverseBSInverse, cAInverse) = args;
                return aInverseBSInverse.MultiplyOnNewThread(cAInverse, cleanupHandlerInternal,
                    memoryAllocationSemaphore, gpuMathsParameters, runningMode,
                    progressHandler);
            });
            var newTopLeftTask = aInverseBInverseCAInverseTask.Join(aInverseTask, (aInverseBInverseCAInverse, aInverse) =>
            {
                return (aInverse, aInverseBInverseCAInverse);
            }).ThenCreateTask(args =>
            {
                var (aInverse, aInverseBInverseCAInverse) = args;
                return aInverse.AddOnNewThread(aInverseBInverseCAInverse, null, memoryAllocationSemaphore);
            });

            // Step 9: Compute new bottom-left: -(S^-1 * C * A^-1) (using MultiplyOnNewThread)
            var newBottomLeftTask = shurComplementInverseTask.Join(cAInverseTask, (shurComplementInverse, cAInverse) =>
            {
                return (shurComplementInverse, cAInverse);
            }).ThenCreateTask(args=>{
                var (shurComplementInverse, cAInverse) = args;
                return shurComplementInverse.ScaleOnNewThread(-1, cleanupHandlerInternal, memoryAllocationSemaphore)
                .Then((scaled)=>(scaled, cAInverse));
            }).ThenCreateTask(args =>
            {
                var (scaled, cAInverse) = args;
                return scaled.MultiplyOnNewThread(cAInverse, null, memoryAllocationSemaphore,
                    gpuMathsParameters, runningMode, progressHandler);
            });

            // Step 10: new bottom-right is just S^-1
            var newBottomRightTask = shurComplementInverseTask;

            // Join all tasks together to form the inverted block matrix
            var inverseTask = newTopLeftTask.Join(newTopRightTask, newBottomLeftTask, newBottomRightTask,
                (newTopLeft, newTopRight, newBottomLeft, newBottomRight) =>
                {
                    cleanupHandlerInternal.Dispose();
                    var result = (BlockOperationMatrix)new BlockOperationMatrixPartitioned(
                        newTopLeft, newTopRight, newBottomLeft, newBottomRight,
                        _WorkingDirectoryManager
                    );
                    if (!cache)
                    {
                        cleanupHandlerCaller?.Add(result);
                    }
                    return result;
                }
            );

            inverseTask.Catch((ex) => cleanupHandlerInternal.Dispose());
            if (cache)
            {
                _InvertTask = inverseTask;
            }
            return inverseTask;
        }

        private BlockOperationMatrixPartitioned CheckPartitioned(BlockOperationMatrix other) {
            if (!other.Partitioned) {
                throw new ArgumentException($"{nameof(BlockOperationMatrix)} {nameof(other)} with path:{other.FilePath} was not partitioned");
            }
            BlockOperationMatrixPartitioned otherPartitioned = (BlockOperationMatrixPartitioned)other;
            return otherPartitioned;
        }
        public override InfernoTaskWithResultBase<BlockOperationMatrix> MultiplyOnNewThread(
            BlockOperationMatrix other, CleanupHandler? cleanupHandlerCaller,
            InfernoFiniteResourceSemaphore memoryAllocationSemaphore,
            GPUMathsParameters? gpuMathsParameters, MathsRunningMode runningMode,
                        CompositeProgressHandler parentProgressHandler)
        {
            if (this.NColumns != other.NRows) throw new IndexOutOfRangeException("size mismatch");
            if (this.NRows != other.NColumns) throw new IndexOutOfRangeException("size mismatch");

            BlockOperationMatrixPartitioned otherPartitioned = CheckPartitioned(other);
            CleanupHandler cleanupHandlerInternal = new CleanupHandler();
            cleanupHandlerCaller?.Add(cleanupHandlerInternal);
            CompositeProgressHandler? progressHandler = null;
            if (parentProgressHandler != null) {
                progressHandler = new CompositeProgressHandler(8);
                parentProgressHandler.AddChild(progressHandler);
            }
            // Step 1: Multiply top-left with other top-left and top-right with other bottom-left
            var topLeftMultTask = ChildTopLeftA.MultiplyOnNewThread(
                otherPartitioned.ChildTopLeftA, cleanupHandlerInternal, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, progressHandler);
            var topRightMultTask = ChildTopRightB.MultiplyOnNewThread(
                otherPartitioned.ChildBottomLeftC, cleanupHandlerInternal, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, progressHandler);

            // Step 2: Add the results for the new A block
            var newATask = topLeftMultTask.Join(topRightMultTask, (topLeftMult, topRightMult) =>
            {
                return (topLeftMult, topRightMult);
            }).ThenCreateTask(args=> {
                var (topLeftMult, topRightMult) = args;
                return topLeftMult.AddOnNewThread(topRightMult, null, memoryAllocationSemaphore);
            });

            // Step 3: Multiply top-left with other top-right and top-right with other bottom-right
            var topLeftRightMultTask = ChildTopLeftA.MultiplyOnNewThread(
                otherPartitioned.ChildTopRightB, cleanupHandlerInternal, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, progressHandler);
            var topRightRightMultTask = ChildTopRightB.MultiplyOnNewThread(
                otherPartitioned.ChildBottomRightD, cleanupHandlerInternal, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, progressHandler);

            // Step 4: Add the results for the new B block
            var newBTask = topLeftRightMultTask.Join(topRightRightMultTask, (topLeftRightMult, topRightRightMult) =>
            {
                return (topLeftRightMult, topRightRightMult);
            }).ThenCreateTask(args => {
                var (topLeftRightMult, topRightRightMult) = args;
                return topLeftRightMult.AddOnNewThread(topRightRightMult, null, memoryAllocationSemaphore);
            });

            // Step 5: Multiply bottom-left with other top-left and bottom-right with other bottom-left
            var bottomLeftMultTask = ChildBottomLeftC.MultiplyOnNewThread(
                otherPartitioned.ChildTopLeftA, cleanupHandlerInternal, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, progressHandler);
            var bottomRightMultTask = ChildBottomRightD.MultiplyOnNewThread(
                otherPartitioned.ChildBottomLeftC, cleanupHandlerInternal, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, progressHandler);

            // Step 6: Add the results for the new C block
            var newCTask = bottomLeftMultTask.Join(bottomRightMultTask, (bottomLeftMult, bottomRightMult) =>
            {
                return (bottomLeftMult, bottomRightMult);
            }).ThenCreateTask(args => {
                var (bottomLeftMult, bottomRightMult) = args;
                return bottomLeftMult.AddOnNewThread(bottomRightMult, null, memoryAllocationSemaphore);
            });

            // Step 7: Multiply bottom-left with other top-right and bottom-right with other bottom-right
            var bottomLeftRightMultTask = ChildBottomLeftC.MultiplyOnNewThread(
                otherPartitioned.ChildTopRightB, cleanupHandlerInternal, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, progressHandler);
            var bottomRightRightMultTask = ChildBottomRightD.MultiplyOnNewThread(
                otherPartitioned.ChildBottomRightD, cleanupHandlerInternal, memoryAllocationSemaphore,
                gpuMathsParameters, runningMode, progressHandler);

            // Step 8: Add the results for the new D block
            var newDTask = bottomLeftRightMultTask.Join(bottomRightRightMultTask, (bottomLeftRightMult, bottomRightRightMult) =>
            {
                return (bottomLeftRightMult, bottomRightRightMult);
            }).ThenCreateTask(args => {
                var (bottomLeftRightMult, bottomRightRightMult) = args;
                return bottomLeftRightMult.AddOnNewThread(bottomRightRightMult, null, memoryAllocationSemaphore);
            });

            // Step 9: After all tasks are done, dispose of intermediate matrices
            var returnTask =  newATask.Join(newBTask, newCTask, newDTask, (newA, newB, newC, newD) =>
            {
                cleanupHandlerInternal.Dispose();
                var result = (BlockOperationMatrix)new BlockOperationMatrixPartitioned(
                    newA, newB, newC, newD, _WorkingDirectoryManager);
                cleanupHandlerCaller?.Add(result);
                return result;
            });
            returnTask.Catch((ex) => cleanupHandlerInternal.Dispose());
            return returnTask;
        }
        public override InfernoTaskWithResultBase<double[]> MultiplyByVectorOnNewThread(
            double[] vector, int vectorOffset, InfernoFiniteResourceSemaphore? memoryAllocationSemaphore)
        {
            var topLeftResultTask = ChildTopLeftA.MultiplyByVectorOnNewThread(vector, vectorOffset, memoryAllocationSemaphore);
            var topRightResultTask = ChildTopRightB.MultiplyByVectorOnNewThread(vector, vectorOffset + ChildTopLeftA.NColumns, memoryAllocationSemaphore);

            var bottomLeftResultTask = ChildBottomLeftC.MultiplyByVectorOnNewThread(vector, vectorOffset, memoryAllocationSemaphore);
            var bottomRightResultTask = ChildBottomRightD.MultiplyByVectorOnNewThread(vector, vectorOffset + ChildTopLeftA.NColumns, memoryAllocationSemaphore);

            // Join the results of the top and bottom tasks
            long memoryRequired = (sizeof(double) * NRows) * 3;
            return memoryAllocationSemaphore.EnterCreateTask(memoryRequired, () =>
                topLeftResultTask.Join(topRightResultTask, bottomLeftResultTask, bottomRightResultTask, (topLeftResult, topRightResult, bottomLeftResult, bottomRightResult) =>
                {
                    double[] results = new double[NRows];

                    // Compute the top part of the result
                    double[] top = VectorHelper.Addition(topLeftResult, topRightResult);
                    Array.Copy(top, 0, results, 0, top.Length);

                    // Compute the bottom part of the result
                    double[] bottom = VectorHelper.Addition(bottomLeftResult, bottomRightResult);
                    Array.Copy(bottom, 0, results, top.Length, bottom.Length);

                    return results;
                })
            );
        }

        public override double[] MultiplyByVector(double[] vector, int vectorOffset) {
            return MultiplyByVectorOnNewThread(vector, vectorOffset, null).Wait();
        }
        public override double[][] ReadIntoMemory()
        {
            double[][] result = new double[NRows][];
            for (int i = 0; i < NRows; i++)
            {
                result[i] = new double[NColumns];
            }
            ReadIntoMemory(result, 0, 0);
            return result;
        }
        public override void ReadIntoMemory(double[][] result, int offsetTop, int offsetLeft)
        {
            ChildTopLeftA.ReadIntoMemory(result, offsetTop, offsetLeft);
            ChildTopRightB.ReadIntoMemory(result, offsetTop, ChildTopLeftA.NColumns + offsetLeft);
            ChildBottomLeftC.ReadIntoMemory(result, ChildTopLeftA.NRows + offsetTop, offsetLeft);
            ChildBottomRightD.ReadIntoMemory(result, ChildTopLeftA.NRows+offsetTop, ChildTopLeftA.NColumns + offsetLeft);
        }
        public override void Dispose()
        {
            base.Dispose();
            ChildTopLeftA.Dispose();
            ChildTopRightB.Dispose();
            ChildBottomLeftC.Dispose();
            ChildBottomRightD.Dispose();
        }
        /*
        public override BlockOperationMatrix Multiply(BlockOperationMatrix other, CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore memoryAllocationSemaphore,
                GPUMathsParameters? gpuMathsParameters,
            MathsRunningMode runningMode)
        {
            return MultiplyOnNewThread(other, cleanupHandler, memoryAllocationSemaphore, 
                gpuMathsParameters, runningMode, null).Wait();
        }*/

        public override InfernoTaskWithResultBase<BlockOperationMatrix> AddOnNewThread(
            BlockOperationMatrix other, CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore? memoryAllocationSemaphore)
        {
            BlockOperationMatrixPartitioned otherPartitioned = CheckPartitioned(other);
            var newATask = ChildTopLeftA.AddOnNewThread(otherPartitioned.ChildTopLeftA, null, memoryAllocationSemaphore);
            var newBTask = ChildTopRightB.AddOnNewThread(otherPartitioned.ChildTopRightB, null, memoryAllocationSemaphore);
            var newCTask = ChildBottomLeftC.AddOnNewThread(otherPartitioned.ChildBottomLeftC, null, memoryAllocationSemaphore);
            var newDTask = ChildBottomRightD.AddOnNewThread(otherPartitioned.ChildBottomRightD, null, memoryAllocationSemaphore);
            return newATask.Join(newBTask, newCTask, newDTask, (newA, newB, newC, newD) =>
            {
                var result = (BlockOperationMatrix)new BlockOperationMatrixPartitioned(newA, newB, newC, newD,
                    _WorkingDirectoryManager);
                cleanupHandler?.Add(result);
                return result;
            });
        }

        public override InfernoTaskWithResultBase<BlockOperationMatrix> SubtractOnNewThread(
            BlockOperationMatrix other, CleanupHandler? cleanupHandler,
            InfernoFiniteResourceSemaphore memoryAllocationSemaphore)
        {
            BlockOperationMatrixPartitioned otherPartitioned = CheckPartitioned(other);
            var newATask = ChildTopLeftA.SubtractOnNewThread(otherPartitioned.ChildTopLeftA, null, memoryAllocationSemaphore);
            var newBTask = ChildTopRightB.SubtractOnNewThread(otherPartitioned.ChildTopRightB, null, memoryAllocationSemaphore);
            var newCTask = ChildBottomLeftC.SubtractOnNewThread(otherPartitioned.ChildBottomLeftC, null, memoryAllocationSemaphore);
            var newDTask = ChildBottomRightD.SubtractOnNewThread(otherPartitioned.ChildBottomRightD, null, memoryAllocationSemaphore);
            return newATask.Join(newBTask, newCTask, newDTask, (newA, newB, newC, newD) =>
            {
                var result = (BlockOperationMatrix)new BlockOperationMatrixPartitioned(newA, newB, newC, newD,
                    _WorkingDirectoryManager);
                cleanupHandler?.Add(result);
                return result;
            });
        }

        public override InfernoTaskWithResultBase<BlockOperationMatrix> ScaleOnNewThread(
            double scale, CleanupHandler? cleanupHandler, InfernoFiniteResourceSemaphore? memoryAllocationSemaphore)
        {
            var taskNewA = ChildTopLeftA.ScaleOnNewThread(scale, null, memoryAllocationSemaphore);
            var taskNewB = ChildTopRightB.ScaleOnNewThread(scale, null, memoryAllocationSemaphore);
            var taskNewC = ChildBottomLeftC.ScaleOnNewThread(scale, null, memoryAllocationSemaphore);
            var taskNewD = ChildBottomRightD.ScaleOnNewThread(scale, null, memoryAllocationSemaphore);
            return taskNewA.Join(taskNewB, taskNewC, taskNewD, (newA, newB, newC, newD) => {

                var result = new BlockOperationMatrixPartitioned(
                    newA, newB, newC, newD, _WorkingDirectoryManager);
                cleanupHandler?.Add(result);
                return (BlockOperationMatrix)result;
            });
        }
    }
}
