using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Globalization;

namespace ARS.WinForms
{
	public static class Util
	{
		//   Validation Methods

		public static bool IsCep(string cep) =>
			MatchesRegex(cep, @"^\d{5}-\d{3}$|^\d{8}$");

		public static bool IsEmail(string email) =>
			MatchesRegex(email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");

		public static bool IsCodigoRastreamento(string codigo) =>
			MatchesRegex(codigo, @"^\D{2}\d+\D{2}$");

		public static bool IsCPF(string cpf)
		{
			cpf = cpf.Trim().Replace(".", "").Replace("-", "");
			if (cpf.Length != 11 || !long.TryParse(cpf, out _)) return false;

			var tempCpf = cpf.Substring(0, 9);
			var digito = CalculateCpfDigit(tempCpf, 10);
			tempCpf += digito;
			digito += CalculateCpfDigit(tempCpf, 11);

			return cpf.EndsWith(digito);
		}

		private static string CalculateCpfDigit(string cpf, int factor)
		{
			int sum = 0;
			for (int i = 0; i < cpf.Length; i++)
				sum += int.Parse(cpf[i].ToString()) * (factor - i);

			int remainder = sum % 11;
			return (remainder < 2 ? 0 : 11 - remainder).ToString();
		}

		public static bool IsCNPJ(string cnpj)
		{
			cnpj = Regex.Replace(cnpj, @"[^\d]", "");
			if (cnpj.Length != 14 || !long.TryParse(cnpj, out _)) return false;

			string weights = "6543298765432";
			int[] digits = new int[14];
			int[] sums = new int[2];

			for (int i = 0; i < 14; i++)
			{
				digits[i] = int.Parse(cnpj[i].ToString());
				if (i <= 11) sums[0] += digits[i] * int.Parse(weights[i + 1].ToString());
				if (i <= 12) sums[1] += digits[i] * int.Parse(weights[i].ToString());
			}

			for (int i = 0; i < 2; i++)
			{
				int result = sums[i] % 11;
				int expectedDigit = result < 2 ? 0 : 11 - result;
				if (digits[12 + i] != expectedDigit) return false;
			}

			return true;
		}

		private static bool MatchesRegex(string input, string pattern) =>
			Regex.IsMatch(input ?? "", pattern, RegexOptions.IgnoreCase);

		// MessageBox Helpers

		public static void Aviso(string message) =>
			MessageBox.Show(message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

		public static void Informar(string message) =>
			MessageBox.Show(message, "Mensagem", MessageBoxButtons.OK, MessageBoxIcon.Information);

		public static void Informar(string message, string caption) =>
			MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);

		public static void Erro(string message) =>
			MessageBox.Show(message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

		[Obsolete("Método foi substituído por \"Prompt\"")]
		public static DialogResult Pergunta(string pergunta) =>
			Prompt(pergunta);

		public static DialogResult Prompt(string pergunta) =>
			MessageBox.Show(pergunta, "Pergunta", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

		//  String Utilities

		public static string ToTitleCase(string str) =>
			CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str?.ToLower() ?? "");

		public static string FirstLetterToUpper(string str) =>
			string.IsNullOrEmpty(str) ? str :
			str.Length == 1 ? str.ToUpper() :
			char.ToUpper(str[0]) + str.Substring(1);
	}
}
