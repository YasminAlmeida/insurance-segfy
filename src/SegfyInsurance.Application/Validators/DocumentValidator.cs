using System.Linq;

namespace SegfyInsurance.Application.Validators
{
    public static class DocumentValidator
    {
        public static bool IsValidCpfCnpj(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            // Remove non-digits
            var cleanValue = new string(value.Where(char.IsDigit).ToArray());

            if (cleanValue.Length == 11)
            {
                return IsValidCpf(cleanValue);
            }
            if (cleanValue.Length == 14)
            {
                return IsValidCnpj(cleanValue);
            }

            return false;
        }

        private static bool IsValidCpf(string cpf)
        {
            if (cpf.Length != 11) return false;

            // Common invalid CPFs
            string[] invalidCpfs = {
                "00000000000", "11111111111", "22222222222", "33333333333",
                "44444444444", "55555555555", "66666666666", "77777777777",
                "88888888888", "99999999999"
            };
            if (invalidCpfs.Contains(cpf)) return false;

            int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int sum = 0;

            for (int i = 0; i < 9; i++)
                sum += int.Parse(tempCpf[i].ToString()) * multiplier1[i];

            int remainder = sum % 11;
            if (remainder < 2)
                remainder = 0;
            else
                remainder = 11 - remainder;

            string digit = remainder.ToString();
            tempCpf = tempCpf + digit;
            sum = 0;

            for (int i = 0; i < 10; i++)
                sum += int.Parse(tempCpf[i].ToString()) * multiplier2[i];

            remainder = sum % 11;
            if (remainder < 2)
                remainder = 0;
            else
                remainder = 11 - remainder;

            digit = digit + remainder.ToString();

            return cpf.EndsWith(digit);
        }

        private static bool IsValidCnpj(string cnpj)
        {
            if (cnpj.Length != 14) return false;

            // Common invalid CNPJs
            string[] invalidCnpjs = {
                "00000000000000", "11111111111111", "22222222222222", "33333333333333",
                "44444444444444", "55555555555555", "66666666666666", "77777777777777",
                "88888888888888", "99999999999999"
            };
            if (invalidCnpjs.Contains(cnpj)) return false;

            int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int sum = 0;

            for (int i = 0; i < 12; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier1[i];

            int remainder = sum % 11;
            if (remainder < 2)
                remainder = 0;
            else
                remainder = 11 - remainder;

            string digit = remainder.ToString();
            tempCnpj = tempCnpj + digit;
            sum = 0;

            for (int i = 0; i < 13; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier2[i];

            remainder = sum % 11;
            if (remainder < 2)
                remainder = 0;
            else
                remainder = 11 - remainder;

            digit = digit + remainder.ToString();

            return cnpj.EndsWith(digit);
        }
    }
}
