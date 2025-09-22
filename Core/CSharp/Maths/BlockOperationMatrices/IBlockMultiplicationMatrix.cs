using System;

namespace Core.Maths.BlockOperationMatrices
{
    public interface IBlockMultiplicationMatrix
    {
        int NRows { get; }
        int NColumns { get; }
        string Name { get; }
        public BlockMultiplicationMatrixType BlockMultiplicationMatrixType { get; }
        void Add(IBlockMultiplicationMatrix other, string resultFilePath);
        void Subtract(IBlockMultiplicationMatrix other, string resultFilePath);
        void Multiply(IBlockMultiplicationMatrix other, string resultFilePath);
        void UsingToGetValues(Action<Func<int, int, double>> callback);
    }
}
