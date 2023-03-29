using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace EquationSolver
{
    public partial class MainPage : ContentPage
    {
        int n; // Number of equations
        List<List<double>> coefficients; // List of coefficients for each equation

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSolveButtonClicked(object sender, EventArgs e)
        {
            // Parse number of equations
            if (!int.TryParse(numEquationsEntry.Text, out n))
            {
                await DisplayAlert("Error", "Please enter a valid number of equations", "OK");
                return;
            }

            // Check that n is between 2 and 5
            if (n < 2 || n > 5)
            {
                await DisplayAlert("Error", "Number of equations must be between 2 and 5", "OK");
                return;
            }

            // Initialize coefficients list
            coefficients = new List<List<double>>();
            for (int i = 0; i < n; i++)
            {
                coefficients.Add(new List<double>());
            }

            // Parse coefficients
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    string coeffStr = await DisplayPromptAsync($"Equation {i+1}", $"Enter coefficient for variable {j+1}");
                    double coeff;
                    if (!double.TryParse(coeffStr, out coeff))
                    {
                        await DisplayAlert("Error", "Please enter a valid number", "OK");
                        return;
                    }
                    coefficients[i].Add(coeff);
                }
            }

            // Solve equations
            List<double> solution = SolveEquations(coefficients);

            // Display solution
            StringBuilder sb = new StringBuilder();
            sb.Append("Solution: ");
            for (int i = 0; i < solution.Count; i++)
            {
                sb.Append($"x{i+1} = {solution[i]} ");
            }
            await DisplayAlert("Solution", sb.ToString(), "OK");
        }

        // Solves a system of linear equations with given coefficients
        private List<double> SolveEquations(List<List<double>> coeffs)
        {
            Matrix matrix = new Matrix(n, n+1);

            // Populate matrix with coefficients
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = coeffs[i][j];
                }
            }

            // Populate matrix with constants
            for (int i = 0; i < n; i++)
            {
                matrix[i, n] = -coeffs[i][n];
            }

        // Populate matrix with constants
            for (int i = 0; i < n; i++)
            {
                string constantStr = await DisplayPromptAsync($"Equation {i+1}", $"Enter constant for equation {i+1}");
                double constant;
                if (!double.TryParse(constantStr, out constant))
                {
                    await DisplayAlert("Error", "Please enter a valid number", "OK");
                    return null;
                }
                matrix[i, n] = -constant;
            }
            
            // Solve matrix
            matrix.GaussianElimination();

            // Extract solution
            List<double> solution = new List<double>();
            for (int i = 0; i < n; i++)
            {
                solution.Add(matrix[i, n]);
            }

            return solution;
        }
    }

        // Represents a matrix of doubles
    class Matrix
    {
        private double[,] data;

        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            data = new double[rows, columns];
        }

        public double this[int row, int column]
        {
            get { return data[row, column]; }
            set { data[row, column] = value; }
        }

        // Solves the matrix using Gaussian elimination
        public void GaussianElimination()
        {
            int rowCount = Rows;
            int colCount = Columns;

            // Forward elimination
            for (int row = 0; row < rowCount - 1; row++)
            {
                int maxRowIndex = row;
                double maxAbsValue = Math.Abs(data[row, row]);
                for (int i = row + 1; i < rowCount; i++)
                {
                    double absValue = Math.Abs(data[i, row]);
                    if (absValue > maxAbsValue)
                    {
                        maxAbsValue = absValue;
                        maxRowIndex = i;
                    }
                }

                if (maxAbsValue < double.Epsilon)
                {
                    throw new Exception("Matrix is singular");
                }

                if (maxRowIndex != row)
                {
                    SwapRows(row, maxRowIndex);
                }

                for (int i = row + 1; i < rowCount; i++)
                {
                    double factor = data[i, row] / data[row, row];
                    for (int j = row + 1; j < colCount; j++)
                    {
                        data[i, j] -= factor * data[row, j];
                    }
                    data[i, row] = 0;
                }
            }

            // Backward substitution
            for (int row = rowCount - 1; row >= 0; row--)
            {
                double sum = 0;
                for (int j = row + 1; j < colCount - 1; j++)
                {
                    sum += data[row, j] * data[j, colCount - 1];
                }
                data[row, colCount - 1] = (data[row, colCount - 1] - sum) / data[row, row];
            }
        }

        private void SwapRows(int row1, int row2)
        {
            for (int j = 0; j < Columns; j++)
            {
                double temp = data[row1, j];
                data[row1, j] = data[row2, j];
                data[row2, j] = temp;
            }
        }
    }
}

