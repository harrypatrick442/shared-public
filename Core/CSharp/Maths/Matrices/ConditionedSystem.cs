using static ManagedCuda.NPP.NPPNativeMethods.NPPi;

namespace Core.Maths.Matrices
{
    public class ConditionedSystem
    {
        public double[][] ScaledMatrix { get; }
        public double[]? ScaledVector { get; }
        public double[] RowNorms { get; }
        public double[] ColumnNorms { get; }
        public double[][] RowScalingMatrix { get; }
        public double[][] ColumnScalingMatrix { get; }

        public ConditionedSystem(
            double[][] scaledMatrix,
            double[]? scaledVector,
            double[] rowNorms,
            double[] columnNorms,
            double[][] rowScalingMatrix,
            double[][] columnScalingMatrix)
        {
            ScaledMatrix = scaledMatrix;
            ScaledVector = scaledVector;
            RowNorms = rowNorms;
            ColumnNorms = columnNorms;
            RowScalingMatrix = rowScalingMatrix;
            ColumnScalingMatrix = columnScalingMatrix;
        }
        public double[] RowScaleVector(double[] vector) {
            return MatrixHelper.MatrixMultiplyByVector(RowScalingMatrix, vector);
        }
        public double[][] ScaleMatrix(double[][] matrix) {
            return MatrixHelper.Multiply(MatrixHelper.Multiply(RowScalingMatrix, matrix), ColumnScalingMatrix);
        }
        public double[] DescaleConditionedMatrixInverseXConditionedVector(double[] scaledSolution)
        {
            int n = scaledSolution.Length;
            double[] descaledSolution = new double[n];

            // Apply both row and column scaling to reverse conditioning
            for (int i = 0; i < n; i++)
            {
                descaledSolution[i] = scaledSolution[i] / ColumnNorms[i];
            }

            return descaledSolution;
        }
        public double[] DescaleConditionedMatrixXConditionedVector(double[] scaledSolution)
        {
            int n = scaledSolution.Length;
            double[] descaledSolution = new double[n];

            // Apply both row and column scaling to reverse conditioning
            for (int i = 0; i < n; i++)
            {
                descaledSolution[i] = scaledSolution[i] * ColumnNorms[i];
            }

            return descaledSolution;
        }


        public double[] RescaleVector(double[] vector)
        {
            int n = vector.Length;
            double[] rescaledVector = new double[n];

            // Apply row scaling
            for (int i = 0; i < n; i++)
            {
                rescaledVector[i] = vector[i] / RowNorms[i];
            }

            return rescaledVector;
        }

    }
}