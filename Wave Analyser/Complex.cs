using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wave_Analyser
{
	class Complex
	{
		public double real;
		public double imaginary;

		public Complex(double real, double imaginary)
		{
			this.real = real;
			this.imaginary = imaginary;
		}

		public static Complex operator +(Complex a, Complex b)
		{
			Complex sum = new Complex(a.real + b.real, a.imaginary + b.imaginary);
			return sum;
		}

		public static Complex operator -(Complex a, Complex b)
		{
			Complex sum = new Complex(a.real - b.real, a.imaginary - b.imaginary);
			return sum;
		}

		public static Complex operator *(Complex a, Complex b)
		{
			Complex sum = new Complex(a.real * b.real - (a.imaginary * b.imaginary),
				a.real * b.imaginary + b.real * a.imaginary);
			return sum;
		}

		public static Complex operator /(Complex a, Complex b)
		{
			double quotient = b.real * b.real + (b.imaginary * b.imaginary);
			Complex sum = new Complex(((a.real * b.real - (a.imaginary * -1 * b.imaginary)) / quotient),
				(a.real * -1 * b.imaginary + b.real * a.imaginary) / quotient);
			return sum;
		}

		public double VectorLength()
		{
			return Math.Pow(real * real + imaginary * imaginary, 0.5);
		}
	}
}

