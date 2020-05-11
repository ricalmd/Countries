using System.Collections.Generic;

namespace Biblioteca
{
    public class Country
    {
        public string Name { get; set; }

        public string Capital { get; set; }

        public string Region { get; set; }

        public string Subregion { get; set; }

        public int Population { get; set; }

        public double Gini { get; set; }

        public string Flag { get; set; }

        public string NumericCode { get; set; }

        public List<Currency> Currencies { get; set; }
    }
}
