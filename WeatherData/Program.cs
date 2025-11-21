using WeatherData;

public class Program
{
    private const string filePath = "../../../TempFuktData.csv";
    private static void Main(string[] args)
    {
		try
		{
			WDDataAccess.InitializeData(filePath);
		}
		catch (Exception ex)
		{
            Console.WriteLine("Oväntat fel!");
            Console.WriteLine(ex.Message);
			throw;
		}
    }
}