using System;
using System.Collections.Generic;

namespace KomiwojazerGA
{
    /// <summary>
    /// Klasa ukazująca trasę
    /// </summary>
    class RouteModel
    {
        public long Distance { get; set; }
        public int[] CitiesIDOrderArr { get; set; }

        /// <summary>
        /// Tworzy obiekt klasy RouteModel
        /// Tworzy tablice miast w kolejności od 1 do MAX ASC, a następnie ją miesza za pomocą algorytmu Fisher-Yates
        /// Dystans jest obliczany na podstawie macierzy <paramref name="distanceArray"/>, gdzie dodaje się odległości sąsiadujących ze sobą ID
        /// </summary>
        /// <param name="distanceArray">Macierz odległości pomiędzy miastami</param>
        /// <param name="cities">Lista obiektów typu "CityModel'</param>
        public RouteModel(int [,] distanceArray, List<CityModel> cities)
        {
            this.CitiesIDOrderArr = new int[cities.Count];
            this.Distance = 0;

            for (int i = 0; i<cities.Count; i++)
            {
                CitiesIDOrderArr[i] = cities[i].ID;
            }

            // Randomize Fisher-Yates algorithm
            int k = 0;
            Random rand = new Random();
            for (int i = CitiesIDOrderArr.Length; i > 1;)
            {
                k = rand.Next(i--);
                int temp = CitiesIDOrderArr[i];
                CitiesIDOrderArr[i] = CitiesIDOrderArr[k];
                CitiesIDOrderArr[k] = temp;
            }

            // Obliczanie całkowitego dystansu
            for(int i = 0; i < CitiesIDOrderArr.Length-1; i++)
            {
                this.Distance += distanceArray[CitiesIDOrderArr[i]-1, CitiesIDOrderArr[i+1]-1];
            }
        }

        public RouteModel(int citiesCount)
        {
            this.CitiesIDOrderArr = new int[citiesCount];
            this.Distance = 0;
        }

        public void setDistance(int[,] distanceArray)
        {
            this.Distance = 0;
            for (int i = 0; i < CitiesIDOrderArr.Length - 1; i++)
            {
                this.Distance += distanceArray[CitiesIDOrderArr[i] - 1, CitiesIDOrderArr[i + 1] - 1];   // '-1' bo ID listy 0,1,2...x, a ID z pliku 1,2,3...x+1
            }
            this.Distance += distanceArray[CitiesIDOrderArr[CitiesIDOrderArr.Length - 1] - 1, CitiesIDOrderArr[0] - 1];
        }
                
        public override String ToString()
        {
            String str = "";
            foreach(var cityID in CitiesIDOrderArr)
            {
                str += cityID + " ";
            }
            str += Distance.ToString();
            return str;
        }
    }
}
