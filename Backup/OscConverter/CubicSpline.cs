using System;

namespace OscConverter
{
    class CubicSpline
    {
        SplineTuple[] splines; // ������

        // ���������, ����������� ������ �� ������ �������� �����
        private struct SplineTuple
        {
            public double a, b, c, d, x;
        }

        // ���������� �������
        // x - ���� �����, ������ ���� ����������� �� �����������, ������� ���� ���������
        // y - �������� ������� � ����� �����
        // n - ���������� ����� �����
        public void BuildSpline(double[] x, double[] y, int n)
        {
            // ������������� ������� ��������
            splines = new SplineTuple[n];
            for (int i = 0; i < n; ++i)
            {
                splines[i].x = x[i];
                splines[i].a = y[i];
            }
            splines[0].c = splines[n - 1].c = 0.0;

            // ������� ���� ������������ ������������� �������� c[i] ������� �������� ��� ���������������� ������
            // ���������� ����������� ������������� - ������ ��� ������ ��������
            double[] alpha = new double[n - 1];
            double[] beta = new double[n - 1];
            alpha[0] = beta[0] = 0.0;
            for (int i = 1; i < n - 1; ++i)
            {
                double hi = x[i] - x[i - 1];
                double hi1 = x[i + 1] - x[i];
                double A = hi;
                double C = 2.0 * (hi + hi1);
                double B = hi1;
                double F = 6.0 * ((y[i + 1] - y[i]) / hi1 - (y[i] - y[i - 1]) / hi);
                double z = (A * alpha[i - 1] + C);
                alpha[i] = -B / z;
                beta[i] = (F - A * beta[i - 1]) / z;
            }

            // ���������� ������� - �������� ��� ������ ��������
            for (int i = n - 2; i > 0; --i)
            {
                splines[i].c = alpha[i] * splines[i + 1].c + beta[i];
            }

            // �� ��������� ������������� c[i] ������� �������� b[i] � d[i]
            for (int i = n - 1; i > 0; --i)
            {
                double hi = x[i] - x[i - 1];
                splines[i].d = (splines[i].c - splines[i - 1].c) / hi;
                splines[i].b = hi * (2.0 * splines[i].c + splines[i - 1].c) / 6.0 + (y[i] - y[i - 1]) / hi;
            }
        }

        // ���������� �������� ����������������� ������� � ������������ �����
        public double Interpolate(double x)
        {
            if (splines == null)
            {
                return double.NaN; // ���� ������� ��� �� ��������� - ���������� NaN
            }

            int n = splines.Length;
            SplineTuple s;

            if (x <= splines[0].x) // ���� x ������ ����� ����� x[0] - ���������� ������ ��-��� �������
            {
                s = splines[0];
            }
            else if (x >= splines[n - 1].x) // ���� x ������ ����� ����� x[n - 1] - ���������� ��������� ��-��� �������
            {
                s = splines[n - 1];
            }
            else // ����� x ����� ����� ���������� ������� ����� - ���������� �������� ����� ������� ��-�� �������
            {
                int i = 0;
                int j = n - 1;
                while (i + 1 < j)
                {
                    int k = i + (j - i) / 2;
                    if (x <= splines[k].x)
                    {
                        j = k;
                    }
                    else
                    {
                        i = k;
                    }
                }
                s = splines[j];
            }

            double dx = x - s.x;
            // ��������� �������� ������� � �������� ����� �� ����� ������� (� ��������, "�����" ���������� �������� �� ����� ������� ���, �� ���� �� ��� ��� ����, ��� �������)
            return s.a + (s.b + (s.c / 2.0 + s.d * dx / 6.0) * dx) * dx;
        }
    }
}