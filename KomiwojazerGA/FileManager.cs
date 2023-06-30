using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KomiwojazerGA
{
    class FileManager
    {
        /// <summary>
        /// Pobiera ścieżki do plików w obecnym folderze
        /// </summary>
        /// <returns>
        /// Listę ścieżek do plików
        /// </returns>
        public List<string> GetAllFilesPaths()
        {
            List<string> paths = new List<string>();
            
            try
            {
                string currentPath = Directory.GetCurrentDirectory();
                Console.WriteLine("The current directory is {0}", currentPath);

                string[] files = Directory.GetFiles(currentPath);
                Console.WriteLine("The number of files in this directory: {0}", files.Length);
/*                foreach (string file in files)
                {
                    Console.WriteLine(file);                    
                }*/
                paths = files.ToList();

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return paths;
        }

        /// <summary>
        /// Czyta plik tekstowy.
        /// Poszukuje odpowiedniego formatu pliku dla stworzenia obiektów typu 'CityModel'  
        /// </summary>
        /// <param name="path">Ścieżka do pliku tekstowego</param>
        /// <param name="numberOfCities">Ilość miast odczytywana z pliku</param>
        /// <returns>
        /// Listę obiektów typu 'CityModel' o rozmiarze <paramref name="numberOfCities"/>  
        /// </returns>
        public List<CityModel> ReadCitiesFromPath(string path, out int numberOfCities)
        {
            List<CityModel> cities = new List<CityModel>();
            numberOfCities = 0;

            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);

                foreach(var line in lines)
                {
                    string modifiedLine = Regex.Replace(line, @"\s+", " ").Trim();

                    string[] lineSplited = modifiedLine.Split(' ');

                    for (int i = 0; i < lineSplited.Length; i++)
                    {
                        bool result0 = Int32.TryParse(lineSplited[i], out int cityID);
                        if(result0)
                        {
                            int cityX = Int32.Parse(lineSplited[++i]);
                            int cityY = Int32.Parse(lineSplited[++i]);

                            CityModel City = new CityModel(cityID, cityX, cityY);
                            cities.Add(City);
                        }
                        else if(lineSplited[i].Equals("DIMENSION"))
                        {
                            i += 2;
                            numberOfCities = Int32.Parse(lineSplited[i]);                            
                        }else if(lineSplited[i].Equals("EOF"))
                        {
                            return cities;
                        }
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return cities;
        }

        public void SaveToFile(RouteModel bestRoute, String fileName)
        {
            //String date = DateTime.Now.ToString("yyyy-dd-M");
            using (StreamWriter stream = File.AppendText("KomiGA-Wyniki_MM_" + fileName + ".txt"))
            {
                stream.WriteLine(bestRoute.ToString());
            }
        }
    }
}
