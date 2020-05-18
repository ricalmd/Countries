using Biblioteca.Models;
using System.Collections.Generic;

namespace Biblioteca
{
    public class Country
    {
        public string Name { get; set; }

        public string Alpha2Code { get; set; }
        
        public string Alpha3Code { get; set; }

        public string Capital { get; set; }

        public string Region { get; set; }

        public string Subregion { get; set; }

        public int Population { get; set; }

        public string Demonym { get; set; }
        
        public double Area { get; set; }

        public double Gini { get; set; }

        public string NativeName { get; set; }

        public Translations Translations { get; set; }

        public string Flag { get; set; }

        public string NumericCode { get; set; }

        public List<Currency> Currencies { get; set; }

        public List<Language> Languages { get; set; }

        public string Cioc { get; set; }
    }
}
