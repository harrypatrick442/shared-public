using Core.Maths.Tensors;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace Core.Maths
{
    public static class ShapeFunctionHelper
    {
        /*
        public static double[][] DeriveBMatrix(double degressFreedomOfNode/*x, y, z*/
        //,
        //double[][] verticesOfNode_s) {

        //N_i(x, y, z) = a_i + (b_i x) + (c_i*y) + (d_i*z)
        //for a tetrahedron with 4 nodes, i = 1 to 4
        //double[] T /*T_i values*/ = new double[][]
        //}
        /*
        public static double[][] ComputeGlobalShapeFunctions(Vector3D v0, Vector3D v1, Vector3D v2, Vector3D v3)
        {
            // Step 1: Construct the matrix M
            double[][] M = new double[4][];
            M[0] = new double[] { v0.X, v0.Y, v0.Z, 1 };
            M[1] = new double[] { v1.X, v1.Y, v1.Z, 1 };
            M[2] = new double[] { v2.X, v2.Y, v2.Z, 1 };
            M[3] = new double[] { v3.X, v3.Y, v3.Z, 1 };

            // Step 2: Invert the matrix M
            double[][] MInverse = MatrixHelper.MatrixInverse(M);

            // Step 3: Extract coefficients for shape functions
            double[][] shapeFunctions = new double[4][];
            for (int i = 0; i < 4; i++)
            {
                shapeFunctions[i] = new double[] { MInverse[i][0], MInverse[i][1], MInverse[i][2], MInverse[i][3] };
            }

            return shapeFunctions;
        }*/
        /*
        public static double[][] GlobalShapeFunctionTetrahedron(Vector3D v1, Vector3D v2, Vector3D v3, Vector3D v4)
        {
            Vector3D[] vectors = new Vector3D[] { v1, v2, v3, v4 };
            double[][] shapeFunctions = new double[4][];
            //Deriving shape function for single node
            double[][] M = new double[4][];
            for (int i = 0; i < 4; i++)
            {
                Vector3D v = vectors[i];
                M[i] = new double[] { 1, v.X, v.Y, v.Z };
            }
            for (int i = 0; i < 4; i++)
            {
                double[] rhs = new double[] { 0, 0, 0, 0 };
                rhs[i] = 1;
                shapeFunctions[i] = MatrixHelper.MatrixMultipliedByVector(MatrixHelper.MatrixInverse(M), rhs);
            }
            double sumOfAllShapeFunctions = 0;

            int testVertexIndex = 1;
            for (int i = 0; i < 4; i++)
            {
                sumOfAllShapeFunctions += VectorHelper.DotProduct(shapeFunctions[i], new double[] { 1, vectors[testVertexIndex].X, vectors[testVertexIndex].Y, vectors[testVertexIndex].Z });
            }
            return shapeFunctions;
        }*/
        public static double[][] GlobalShapeFunctionTetrahedron(Vector3D v1, Vector3D v2, Vector3D v3, Vector3D v4)
        {
            // Assemble the global coordinate matrix for the tetrahedron using double[][]
            double[][] G = new double[4][]
            {
        new double[] { 1, v1.X, v1.Y, v1.Z },
        new double[] { 1, v2.X, v2.Y, v2.Z },
        new double[] { 1, v3.X, v3.Y, v3.Z },
        new double[] { 1, v4.X, v4.Y, v4.Z }
            };

            // Invert the matrix G
            double[][] G_inv = MatrixHelper.Invert(G);

            // Each row of G_inv represents the coefficients of the shape functions N1, N2, N3, N4
            double[][] globalShapeFunctions = new double[4][];
            for (int i = 0; i < 4; i++)
            {
                globalShapeFunctions[i] = new double[4]
                {
            G_inv[i][0], // Constant term
            G_inv[i][1], // Coefficient of X
            G_inv[i][2], // Coefficient of Y
            G_inv[i][3]  // Coefficient of Z
                };
            }

            return globalShapeFunctions;
        }

        public static double[][] ShapeFunctionTetrahedron(Vector3D v1, Vector3D v2, Vector3D v3, Vector3D v4)
        {
            // Create the matrix M using the coordinates of the four vertices of the tetrahedron
            double[][] M = new double[4][]
            {
        new double[] { 1, v1.X, v1.Y, v1.Z },
        new double[] { 1, v2.X, v2.Y, v2.Z },
        new double[] { 1, v3.X, v3.Y, v3.Z },
        new double[] { 1, v4.X, v4.Y, v4.Z }
            };

            // Initialize an array to store the shape function coefficients for each node
            double[][] shapeFunctions = new double[4][];

            // Loop over each node to compute the shape function coefficients
            for (int i = 0; i < 4; i++)
            {
                // Set up the RHS vector (1 at the current node, 0 for the others)
                double[] rhs = new double[] { 0, 0, 0, 0 };
                rhs[i] = 1;  // Set the i-th entry to 1, others are 0

                // Invert the matrix M (4x4) and multiply by the RHS vector to get the coefficients
                double[] Ci = MatrixHelper.MatrixMultiplyByVector(MatrixHelper.Invert(M), rhs);

                // Store the coefficients for this shape function
                shapeFunctions[i] = Ci;
            }

            return shapeFunctions;  // Return the coefficients for the shape functions
        }
        public static double[][] ComputeTetrahedronBMatrix3DOF3FieldComponentsGradients(
    double[][] shapeFunctions)
        {
            // Extract the shape function derivatives for each node
            double[] N1 = shapeFunctions[0];
            double[] N2 = shapeFunctions[1];
            double[] N3 = shapeFunctions[2];
            double[] N4 = shapeFunctions[3];

            // Use the derivatives (b1, c1, d1 for each node)
            double b1 = N1[1], b2 = N2[1], b3 = N3[1], b4 = N4[1]; // dN/dx
            double c1 = N1[2], c2 = N2[2], c3 = N3[2], c4 = N4[2]; // dN/dy
            double d1 = N1[3], d2 = N2[3], d3 = N3[3], d4 = N4[3]; // dN/dz

            // Construct the B matrix for 3 DOFs (Ax, Ay, Az) and 3 field components
            double[][] B = new double[][]
            {
        new double[]{ b1, 0,  0,  b2, 0,  0,  b3, 0,  0,  b4, 0,  0  },
        new double[]{ 0,  c1, 0,  0,  c2, 0,  0,  c3, 0,  0,  c4, 0  },
        new double[]{ 0,  0,  d1, 0,  0,  d2, 0,  0,  d3, 0,  0,  d4 }
            };

            return B;
        }

        public static double[][] ComputeTetrahedronBMatrix3DOF6FieldComponentsUsingGradient(
           double[][] shapeFunctions)
        {
            double[] N1 = shapeFunctions[0];
            double[] N2 = shapeFunctions[1];
            double[] N3 = shapeFunctions[2];
            double[] N4 = shapeFunctions[3];
            double b1 = N1[1];
            double b2 = N2[1];
            double b3 = N3[1];
            double b4 = N4[1];
            double c1 = N1[2];
            double c2 = N2[2];
            double c3 = N3[2];
            double c4 = N4[2];
            double d1 = N1[3];
            double d2 = N2[3];
            double d3 = N3[3];
            double d4 = N4[3];
            double[][] B = new double[][] {
                new double[]{ b1, 0,  0,  b2, 0,  0,  b3, 0,  0,  b4, 0,  0  },
                new double[]{ 0,  c1, 0,  0,  c2, 0,  0,  c3, 0,  0,  c4, 0  },
                new double[]{ 0,  0,  d1, 0,  0,  d2, 0,  0,  d3, 0,  0,  d4 },
                new double[]{ c1, b1, 0,  c2, b2, 0,  c3, b3, 0,  c4, b4, 0  },
                new double[]{ 0,  d1, c1, 0,  d2, c2, 0,  d3, c3, 0,  d4, c4 },
                new double[]{ d1, 0,  b1, d2, 0,  b2, d3, 0,  b3, d4, 0,  b4 }
            };
            return B;
        }
        public static double[][] ComputeTetrahedronBMatrix3DOF9FieldComponentsUsingGradient(double[][] shapeFunctions)
        {
            double[] N1 = shapeFunctions[0];
            double[] N2 = shapeFunctions[1];
            double[] N3 = shapeFunctions[2];
            double[] N4 = shapeFunctions[3];

            // Partial derivatives of the shape functions for each node
            double b1 = N1[0], c1 = N1[1], d1 = N1[2];
            double b2 = N2[0], c2 = N2[1], d2 = N2[2];
            double b3 = N3[0], c3 = N3[1], d3 = N3[2];
            double b4 = N4[0], c4 = N4[1], d4 = N4[2];

            // Initialize the 9x12 B matrix (3 DOFs per node)
            double[][] B = new double[][]
            {
            // dN/dx for A_x, A_y, and A_z
            new double[] { b1, 0, 0, b2, 0, 0, b3, 0, 0, b4, 0, 0 },  // dN/dx for A_x
            new double[] { 0, c1, 0, 0, c2, 0, 0, c3, 0, 0, c4, 0 },  // dN/dx for A_y
            new double[] { 0, 0, d1, 0, 0, d2, 0, 0, d3, 0, 0, d4 },  // dN/dx for A_z

            // dN/dy for A_x, A_y, and A_z
            new double[] { c1, 0, 0, c2, 0, 0, c3, 0, 0, c4, 0, 0 },  // dN/dy for A_x
            new double[] { 0, b1, 0, 0, b2, 0, 0, b3, 0, 0, b4, 0 },  // dN/dy for A_y
            new double[] { 0, 0, c1, 0, 0, c2, 0, 0, c3, 0, 0, c4 },  // dN/dy for A_z

            // dN/dz for A_x, A_y, and A_z
            new double[] { d1, 0, 0, d2, 0, 0, d3, 0, 0, d4, 0, 0 },  // dN/dz for A_x
            new double[] { 0, d1, 0, 0, d2, 0, 0, d3, 0, 0, d4, 0 },  // dN/dz for A_y
            new double[] { 0, 0, b1, 0, 0, b2, 0, 0, b3, 0, 0, b4 },  // dN/dz for A_z
            };

            return B;
        }
    public static double[][] ComputeScalarBMatrixForTetrahedronElementUsingGradient(double[][] shapeFunctions)
        {

            double[] N1 = shapeFunctions[0];
            double[] N2 = shapeFunctions[1];
            double[] N3 = shapeFunctions[2];
            double[] N4 = shapeFunctions[3];
            double b1 = N1[1];
            double b2 = N2[1];
            double b3 = N3[1];
            double b4 = N4[1];
            double c1 = N1[2];
            double c2 = N2[2];
            double c3 = N3[2];
            double c4 = N4[2];
            double d1 = N1[3];
            double d2 = N2[3];
            double d3 = N3[3];
            double d4 = N4[3]; 
            double[][] BScalar = new double[][]
            {
                new double[] { b1, b2, b3, b4 }, // ∂N/∂x (Gradient of shape functions with respect to x)
                new double[] { c1, c2, c3, c4 }, // ∂N/∂y (Gradient of shape functions with respect to y)
                new double[] { d1, d2, d3, d4 }, // ∂N/∂z (Gradient of shape functions with respect to z)
            };
            return BScalar;
        }
        public static double[][] ComputeTetrahedronBMatrix3DOF3FieldComponentsCurl(double[][] shapeFunctions)
        {
            // Extract the shape function derivatives for each node
            double[] N1 = shapeFunctions[0];
            double[] N2 = shapeFunctions[1];
            double[] N3 = shapeFunctions[2];
            double[] N4 = shapeFunctions[3];

            // Use the derivatives (b1, c1, d1 for each node)
            double b1 = N1[1], b2 = N2[1], b3 = N3[1], b4 = N4[1]; // dN/dx
            double c1 = N1[2], c2 = N2[2], c3 = N3[2], c4 = N4[2]; // dN/dy
            double d1 = N1[3], d2 = N2[3], d3 = N3[3], d4 = N4[3]; // dN/dz

            // Construct the B matrix for 3 DOFs (Ax, Ay, Az) and 3 curl components
            double[][] B = new double[][]
            {
                // Curl X component (d/dy A_z - d/dz A_y)
                new double[]{ 0, -d1, c1, 0, -d2, c2, 0, -d3, c3, 0, -d4, c4 },
        
                // Curl Y component (d/dz A_x - d/dx A_z)
                new double[]{ d1, 0, -b1, d2, 0, -b2,  d3, 0, -b3, d4, 0, -b4 },
        
                // Curl Z component (d/dx A_y - d/dy A_x)
                new double[]{ -c1, b1, 0, -c2, b2, 0, -c3, b3, 0, -c4, b4, 0 }
            };

            return B;
        }

        public static double[][] ComputeTetrahedronBMatrix3DOF6FieldComponentsStrainDisplacement(double[][] shapeFunctions)
        {
            double[] N1 = shapeFunctions[0];
            double[] N2 = shapeFunctions[1];
            double[] N3 = shapeFunctions[2];
            double[] N4 = shapeFunctions[3];
            // Extract shape function derivatives for each node
            double b1 = N1[1], c1 = N1[2], d1 = N1[3];
            double b2 = N2[1], c2 = N2[2], d2 = N2[3];
            double b3 = N3[1], c3 = N3[2], d3 = N3[3];
            double b4 = N4[1], c4 = N4[2], d4 = N4[3];

            // Strain-displacement B matrix (6 strains, 12 displacement DOFs for tetrahedral elements)
            double[][] B = new double[6][];

            B[0] = new double[] { b1, 0, 0, b2, 0, 0, b3, 0, 0, b4, 0, 0 };              // ε_xx = du/dx
            B[1] = new double[] { 0, c1, 0, 0, c2, 0, 0, c3, 0, 0, c4, 0 }; // ε_yy = dv/dy
            B[2] = new double[] { 0, 0, d1, 0, 0, d2, 0, 0, d3, 0, 0, d4 };  // ε_zz = dw/dz

            B[3] = new double[] { c1, b1, 0, c2, b2, 0, c3, b3, 0, c4, b4, 0 }; // γ_xy = du/dy + dv/dx
            B[4] = new double[] { 0, d1, c1, 0, d2, c2, 0, d3, c3, 0, d4, c4 };  // ε_yz = dv/dz + dw/dy
            B[5] = new double[] { d1, 0, b1, d2, 0, b2, d3, 0, b3, d4, 0, b4 };  // ε_xz = du/dz + dw/dx

            return B;
        }
    }
}