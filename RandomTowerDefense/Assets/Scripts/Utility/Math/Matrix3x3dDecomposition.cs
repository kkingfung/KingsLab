﻿
using System;
using UnityEngine;

namespace LinearAlgebra
{

    public static class Matrix3x3dDecomposition
    {
        /// <summary>
        /// Rotates A through phi in pq-plane to set A(p,q) = 0
	    /// Rotation stored in R whose columns are eigenvectors of A
        /// </summary>
        public static void JacobiRotate(ref Matrix3x3d A, ref Matrix3x3d R, int p, int q)
        {

            if (A[p, q] == 0.0f)
                return;

            float d = (A[p, p] - A[q, q]) / (2.0f * A[p, q]);
            float t = 1.0f / (Mathf.Abs(d) + Mathf.Sqrt(d * d + 1.0f));
            if (d < 0.0f) t = -t;
            float c = 1.0f / Mathf.Sqrt(t * t + 1);
            float s = t * c;

            A[p, p] += t * A[p, q];
            A[q, q] -= t * A[p, q];
            A[p, q] = A[q, p] = 0.0f;

            // transform A
            int k;
            for (k = 0; k < 3; k++)
            {
                if (k != p && k != q)
                {
                    float Akp = c * A[k, p] + s * A[k, q];
                    float Akq = -s * A[k, p] + c * A[k, q];
                    A[k, p] = A[p, k] = Akp;
                    A[k, q] = A[q, k] = Akq;
                }
            }

            // store rotation in R
            for (k = 0; k < 3; k++)
            {
                float Rkp = c * R[k, p] + s * R[k, q];
                float Rkq = -s * R[k, p] + c * R[k, q];
                R[k, p] = Rkp;
                R[k, q] = Rkq;
            }
        }

        public static void EigenDecomposition(Matrix3x3d A, out Matrix3x3d eigenVecs, out Vector3 eigenVals)
        {

            const int numJacobiIterations = 10;
            const float epsilon = 1e-15f;

            Matrix3x3d D = A;

            // only for symmetric matrices!
            eigenVecs = Matrix3x3d.Identity;    // unit matrix
            int iter = 0;

            while (iter < numJacobiIterations)
            {
                // 3 off diagonal elements
                // find off diagonal element with maximum modulus
                int p, q;
                float a, max;
                max = Mathf.Abs(D.m01);
                p = 0; q = 1;
                a = Mathf.Abs(D.m02);
                if (a > max) { p = 0; q = 2; max = a; }
                a = Mathf.Abs(D.m12);
                if (a > max) { p = 1; q = 2; max = a; }
                // all small enough -> done
                if (max < epsilon) break;
                // rotate matrix with respect to that element
                JacobiRotate(ref D, ref eigenVecs, p, q);
                iter++;
            }

            eigenVals = new Vector3(D.m00, D.m11, D.m22);
        }

        /// <summary>
        /// Perform polar decomposition A = (U D U^T) R
        /// </summary>
        public static void PolarDecomposition(Matrix3x3d A, out Matrix3x3d R, out Matrix3x3d U, out Matrix3x3d D)
        {
            // A = SR, where S is symmetric and R is orthonormal
            // -> S = (A A^T)^(1/2)

            // A = U D U^T R

            Matrix3x3d AAT = new Matrix3x3d();
            AAT.m00 = A.m00 * A.m00 + A.m01 * A.m01 + A.m02 * A.m02;
            AAT.m11 = A.m10 * A.m10 + A.m11 * A.m11 + A.m12 * A.m12;
            AAT.m22 = A.m20 * A.m20 + A.m21 * A.m21 + A.m22 * A.m22;

            AAT.m01 = A.m00 * A.m10 + A.m01 * A.m11 + A.m02 * A.m12;
            AAT.m02 = A.m00 * A.m20 + A.m01 * A.m21 + A.m02 * A.m22;
            AAT.m12 = A.m10 * A.m20 + A.m11 * A.m21 + A.m12 * A.m22;

            AAT.m10 = AAT.m01;
            AAT.m20 = AAT.m02;
            AAT.m21 = AAT.m12;

            R = Matrix3x3d.Identity;
            Vector3 eigenVals;
            EigenDecomposition(AAT, out U, out eigenVals);

            float d0 = Mathf.Sqrt(eigenVals.x);
            float d1 = Mathf.Sqrt(eigenVals.y);
            float d2 = Mathf.Sqrt(eigenVals.z);

            D = new Matrix3x3d();
            D.m00 = d0;
            D.m11 = d1;
            D.m22 = d2;

            const float eps = 1e-15f;

            float l0 = eigenVals.x; if (l0 <= eps) l0 = 0.0f; else l0 = 1.0f / d0;
            float l1 = eigenVals.y; if (l1 <= eps) l1 = 0.0f; else l1 = 1.0f / d1;
            float l2 = eigenVals.z; if (l2 <= eps) l2 = 0.0f; else l2 = 1.0f / d2;

            Matrix3x3d S1 = new Matrix3x3d();
            S1.m00 = l0 * U.m00 * U.m00 + l1 * U.m01 * U.m01 + l2 * U.m02 * U.m02;
            S1.m11 = l0 * U.m10 * U.m10 + l1 * U.m11 * U.m11 + l2 * U.m12 * U.m12;
            S1.m22 = l0 * U.m20 * U.m20 + l1 * U.m21 * U.m21 + l2 * U.m22 * U.m22;

            S1.m01 = l0 * U.m00 * U.m10 + l1 * U.m01 * U.m11 + l2 * U.m02 * U.m12;
            S1.m02 = l0 * U.m00 * U.m20 + l1 * U.m01 * U.m21 + l2 * U.m02 * U.m22;
            S1.m12 = l0 * U.m10 * U.m20 + l1 * U.m11 * U.m21 + l2 * U.m12 * U.m22;

            S1.m10 = S1.m01;
            S1.m20 = S1.m02;
            S1.m21 = S1.m12;

            R = S1 * A;

            // stabilize
            Vector3 c0, c1, c2;
            c0 = R.GetColumn(0);
            c1 = R.GetColumn(1);
            c2 = R.GetColumn(2);

            if (Vector3.SqrMagnitude(c0) < eps)
                c0 = Vector3.Cross(c1,c2);
            else if (Vector3.SqrMagnitude(c1) < eps)
                c1 = Vector3.Cross(c2,c0);
            else
                c2 = Vector3.Cross(c0, c1);

            R.SetColumn(0, c0);
            R.SetColumn(1, c1);
            R.SetColumn(2, c2);
        }

        /// <summary>
        /// Return the one norm of the matrix.
        /// </summary>
        public static float OneNorm(Matrix3x3d A)
        {
            float sum1 = Mathf.Abs(A.m00) + Mathf.Abs(A.m10) + Mathf.Abs(A.m20);
            float sum2 = Mathf.Abs(A.m01) + Mathf.Abs(A.m11) + Mathf.Abs(A.m21);
            float sum3 = Mathf.Abs(A.m02) + Mathf.Abs(A.m12) + Mathf.Abs(A.m22);

            float maxSum = sum1;

            if (sum2 > maxSum)
                maxSum = sum2;
            if (sum3 > maxSum)
                maxSum = sum3;

            return maxSum;
        }

        /// <summary>
        /// Return the inf norm of the matrix.
        /// </summary>
        public static float InfNorm(Matrix3x3d A)
        {
            float sum1 = Mathf.Abs(A.m00) + Mathf.Abs(A.m01) + Mathf.Abs(A.m02);
            float sum2 = Mathf.Abs(A.m10) + Mathf.Abs(A.m11) + Mathf.Abs(A.m12);
            float sum3 = Mathf.Abs(A.m20) + Mathf.Abs(A.m21) + Mathf.Abs(A.m22);

            float maxSum = sum1;

            if (sum2 > maxSum)
                maxSum = sum2;
            if (sum3 > maxSum)
                maxSum = sum3;

            return maxSum;
        }

        /// <summary>
        /// Perform a polar decomposition of matrix M and return the rotation matrix R. This method handles the degenerated cases.
        /// </summary>am>
        public static void PolarDecompositionStable(Matrix3x3d M, float tolerance, out Matrix3x3d R)
        {
            Matrix3x3d Mt = M.Transpose;
            float Mone = OneNorm(M);
            float Minf = InfNorm(M);
            float Eone;
            Matrix3x3d MadjTt = new Matrix3x3d();
            Matrix3x3d Et = new Matrix3x3d();

            const float eps = 1.0e-15f;

            do
            {
                MadjTt.SetRow(0, Vector3.Cross(Mt.GetRow(1), Mt.GetRow(2)));
                MadjTt.SetRow(1, Vector3.Cross(Mt.GetRow(2), Mt.GetRow(0)));
                MadjTt.SetRow(2, Vector3.Cross(Mt.GetRow(0), Mt.GetRow(1)));

                float det = Mt.m00 * MadjTt.m00 + Mt.m01 * MadjTt.m01 + Mt.m02 * MadjTt.m02;

                if (Mathf.Abs(det) < eps)
                {
                    int index = int.MaxValue;
                    for (int i = 0; i < 3; ++i)
                    {
                        float len = Vector3.SqrMagnitude(MadjTt.GetRow(i));
                        if (len > eps)
                        {
                            // index of valid cross product
                            // => is also the index of the vector in Mt that must be exchanged
                            index = i;
                            break;
                        }
                    }

                    if (index == int.MaxValue)
                    {
                        R = Matrix3x3d.Identity;
                        return;
                    }
                    else
                    {
                        Mt.SetRow(index, Vector3.Cross(Mt.GetRow((index + 1) % 3), Mt.GetRow((index + 2) % 3)));
                        MadjTt.SetRow((index + 1) % 3, Vector3.Cross(Mt.GetRow((index + 2) % 3), Mt.GetRow(index)));
                        MadjTt.SetRow((index + 2) % 3, Vector3.Cross(Mt.GetRow(index), Mt.GetRow((index + 1) % 3)));
                        Matrix3x3d M2 = Mt.Transpose;

                        Mone = OneNorm(M2);
                        Minf = InfNorm(M2);

                        det = Mt.m00 * MadjTt.m00 + Mt.m01 * MadjTt.m01 + Mt.m02 * MadjTt.m02;
                    }
                }

                float MadjTone = OneNorm(MadjTt);
                float MadjTinf = InfNorm(MadjTt);

                float gamma = Mathf.Sqrt(Mathf.Sqrt((MadjTone * MadjTinf) / (Mone * Minf)) / Mathf.Abs(det));

                float g1 = gamma * 0.5f;
                float g2 = 0.5f / (gamma * det);

                for (int i = 0; i < 3; ++i)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Et[i, j] = Mt[i, j];
                        Mt[i, j] = g1 * Mt[i, j] + g2 * MadjTt[i, j];
                        Et[i, j] -= Mt[i, j];
                    }
                }

                Eone = OneNorm(Et);

                Mone = OneNorm(Mt);
                Minf = InfNorm(Mt);
            }
            while (Eone > Mone * tolerance);

            // Q = Mt^T 
            R = Mt.Transpose;

            //end of function
        }


        /// <summary>
        /// Perform a singular value decomposition of matrix A: A = U * sigma * V^T.
        /// This function returns two proper rotation matrices U and V^T which do not 
        /// contain a reflection. Reflections are corrected by the inversion handling
        /// proposed by Irving et al. 2004.
        /// </summary>
        public static void SVDWithInversionHandling(Matrix3x3d A, out Vector3 sigma, out Matrix3x3d U, out Matrix3x3d VT)
        {

            Vector3 S;
            Matrix3x3d AT_A, V;

            AT_A = A.Transpose * A;

            // Eigen decomposition of A^T * A
            EigenDecomposition(AT_A, out V, out S);
            int pos;
            // Detect if V is a reflection .
            // Make a rotation out of it by multiplying one column with -1.
            float detV = V.Determinant;
            if (detV < 0.0f)
            {
                float minLambda = float.PositiveInfinity;
                pos = 0;
                for (int l = 0; l < 3; l++)
                {
                    if (S[l] < minLambda)
                    {
                        pos = l;
                        minLambda = S[l];
                    }
                }
                V[0, pos] = -V[0, pos];
                V[1, pos] = -V[1, pos];
                V[2, pos] = -V[2, pos];
            }

            if (S.x < 0.0f) S.x = 0.0f;     // safety for sqrt
            if (S.y < 0.0f) S.y = 0.0f;
            if (S.z < 0.0f) S.z = 0.0f;

            sigma.x = Mathf.Sqrt(S.x);
            sigma.y = Mathf.Sqrt(S.y);
            sigma.z = Mathf.Sqrt(S.z);

            VT = V.Transpose;

            // Check for values of hatF near zero
            int chk = 0;
            pos = 0;
            for (int l = 0; l < 3; l++)
            {
                if (Mathf.Abs(sigma[l]) < 1.0e-4)
                {
                    pos = l;
                    chk++;
                }
            }

            if (chk > 0)
            {

                if (chk > 1)
                {
                    U = Matrix3x3d.Identity;
                }
                else
                {
                    U = A * V;
                    for (int l = 0; l < 3; l++)
                    {
                        if (l != pos)
                        {
                            for (int m = 0; m < 3; m++)
                            {
                                U[m, l] *= 1.0f / sigma[l];

                            }
                        }
                    }

                    Vector3[] v = new Vector3[2];
                    int index = 0;
                    for (int l = 0; l < 3; l++)
                    {
                        if (l != pos)
                        {
                            v[index++] = new Vector3(U[0, l], U[1, l], U[2, l]);
                        }
                    }

                    Vector3 vec = Vector3.Cross(v[0], v[1]);
                    vec.Normalize();
                    U[0, pos] = vec[0];
                    U[1, pos] = vec[1];
                    U[2, pos] = vec[2];
                }

            }
            else
            {
                Vector3 sigmaInv = new Vector3(1.0f / sigma.x, 1.0f / sigma.y, 1.0f / sigma.z);

                U = A * V;
                for (int l = 0; l < 3; l++)
                {
                    for (int m = 0; m < 3; m++)
                    {
                        U[m, l] *= sigmaInv[l];
                    }
                }
            }

            float detU = U.Determinant;

            // U is a reflection => inversion
            if (detU < 0.0)
            {
                //std::cout << "Inversion!\n";
                float minLambda = float.PositiveInfinity;
                pos = 0;
                for (int l = 0; l < 3; l++)
                {
                    if (sigma[l] < minLambda)
                    {
                        pos = l;
                        minLambda = sigma[l];
                    }
                }

                // invert values of smallest singular value
                sigma[pos] = -sigma[pos];
                U[0, pos] = -U[0, pos];
                U[1, pos] = -U[1, pos];
                U[2, pos] = -U[2, pos];
            }

            //end of function
        }

    }

}