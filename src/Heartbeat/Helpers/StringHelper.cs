namespace Heartbeat.Helpers
{
    public static class StringHelper
    {
        public static int GetInt(string param1)
        {
            if (double.TryParse(param1, out var tmp))
            {
                return (int)tmp;
            }

            return int.TryParse(param1, out var tmp2) ? tmp2 : 0;
        }
        public static double GetDouble(string param1)
        {
            if (double.TryParse(param1, out var tmp))
            {
                return tmp;
            }

            return int.TryParse(param1, out var tmp2) ? tmp2 : 0;
        }
    }
}
