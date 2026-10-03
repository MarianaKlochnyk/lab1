using System;
using System.IO;
using OxyPlot.Wpf;
using ScottPlot;

class Program
{
    const int n = 4;

    const double A = -Math.PI;
    const double B = Math.PI;

    const int IntegrationPoints = 10000;

    const int GraphPoints = 1000;

    const int N = 50;

    static double Function(double x)
    {
        return Math.Pow(x, 4) * Math.Exp(-Math.Pow(x, 2) / 4);
    }


    static double Integrate(Func<double, double> func)
    {
        double h = (B - A) / IntegrationPoints;
        double sum = 0;

        for (int i = 0; i <= IntegrationPoints; i++)
        {
            double x = A + i * h;
            double value = func(x);

            if (i == 0 || i == IntegrationPoints)
                sum += value / 2;
            else
                sum += value;
        }

        return sum * h;
    }

    static double CalculateA0()
    {
        return (1 / Math.PI) * Integrate(Function);
    }

    static double CalculateAk(int k)
    {
        return (1 / Math.PI) * Integrate(x => Function(x) * Math.Cos(k * x));
    }

    static double CalculateBk(int k)
    {
        return (1 / Math.PI) * Integrate(x => Function(x) * Math.Sin(k * x));
    }

    static double FourierApproximation(double x, int order, double a0, double[] ak, double[] bk){
        double result = a0 / 2;

        for (int k = 1; k <= order; k++)
        {
            result += ak[k] * Math.Cos(k * x) + bk[k] * Math.Sin(k * x);
        }

        return result;
    }

    static double RelativeError(double exact, double approximation)
    {
        return Math.Abs(exact - approximation) / Math.Abs(exact);
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        double[] ak = new double[N + 1];
        double[] bk = new double[N + 1];

        double a0 = CalculateA0();

        Console.WriteLine($"a0 = {a0:F10}");
        Console.WriteLine();

        Console.WriteLine("Коефіцієнти Фур'є:");

        for (int k = 1; k <= N; k++)
        {
            ak[k] = CalculateAk(k);
            bk[k] = CalculateBk(k);

            Console.WriteLine($"k = {k,2}:  a{k} = {ak[k]:F10},  b{k} = {bk[k]:F10}");
        }

        double maxError = 0;

        for (int i = 0; i <= GraphPoints; i++)
        {
            double x = A + (B - A) * i / GraphPoints;

            double exact = Function(x);

            double approximation = FourierApproximation(x, N, a0, ak, bk);

            if (Math.Abs(exact) >= 1e-3)
            {
                double error = RelativeError(exact, approximation);

                if (error > maxError)
                    maxError = error;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Максимальна відносна похибка = {maxError:F10}");

        double[] xValues = new double[GraphPoints + 1];
        double[] exactValues = new double[GraphPoints + 1];
        double[] approximationValues = new double[GraphPoints + 1];

        for (int i = 0; i <= GraphPoints; i++)
        {
            double x = A + (B - A) * i / GraphPoints;

            xValues[i] = x;
            exactValues[i] = Function(x);

            approximationValues[i] = FourierApproximation(x, N, a0, ak, bk);
        }

        var plot = new Plot();

        var exactPlot = plot.Add.Scatter(xValues, exactValues);

        exactPlot.LegendText = "Точна функція";

        var approximationPlot = plot.Add.Scatter(xValues, approximationValues);

        approximationPlot.LegendText = "Наближення Фур'є";

        plot.Title("Наближення функції тригонометричним рядом Фур'є");

        plot.XLabel("x");
        plot.YLabel("f(x)");

        plot.ShowLegend();

        plot.SavePng("fourier_approximation.png", 1200, 800);

        double[] kValues = new double[N + 1];
        double[] aValues = new double[N + 1];
        double[] bValues = new double[N + 1];

        kValues[0] = 0;
        aValues[0] = a0;
        bValues[0] = 0;

        for (int k = 1; k <= N; k++)
        {
            kValues[k] = k;
            aValues[k] = ak[k];
            bValues[k] = bk[k];
        }

        var coefficientPlot = new Plot();

        var aPlot = coefficientPlot.Add.Scatter(kValues, aValues);

        aPlot.LegendText = "aₖ";

        var bPlot = coefficientPlot.Add.Scatter(kValues, bValues);

        bPlot.LegendText = "bₖ";

        coefficientPlot.Title("Коефіцієнти ряду Фур'є");

        coefficientPlot.XLabel("k");
        coefficientPlot.YLabel("Коефіцієнти");

        coefficientPlot.ShowLegend();

        coefficientPlot.SavePng("fourier_coefficients.png", 1200, 800);

        using (StreamWriter writer = new StreamWriter("fourier_results.txt"))
        {
            writer.WriteLine("Наближення функції тригонометричним рядом Фур'є");

            writer.WriteLine($"n = {n}");
            writer.WriteLine($"N = {N}");

            writer.WriteLine($"a0 = {a0:F10}");

            writer.WriteLine("k\tak\t\tbk");

            for (int k = 1; k <= N; k++)
            {
                writer.WriteLine($"{k}\t{ak[k]:F10}\t{bk[k]:F10}");
            }

            writer.WriteLine($"Максимальна відносна похибка = {maxError:F10}");
        }

        Console.WriteLine("Натисніть Enter для завершення...");
    }
}