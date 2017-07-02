using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wczytywanie_ZPliku
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        int[][] daneZPliku;

        private void btnWybierz_Click(object sender, EventArgs e)
        {
            var wynik = ofd.ShowDialog();
            if (wynik != DialogResult.OK)
                return;


            if (wynik == DialogResult.OK)
            {
                //wyczyść
                textBoxFisher.Clear();


                tbScieszka.Text = ofd.FileName;
                string trescPliku = System.IO.File.ReadAllText(ofd.FileName);

                string[] poziomy = trescPliku.Split('\n');

                daneZPliku = new int[poziomy.Length][];

                for (int i = 0; i < poziomy.Length; i++)
                {
                    string poziom = poziomy[i].Trim();
                    string[] miejscaParkingowe = poziom.Split(' ');
                    daneZPliku[i] = new int[miejscaParkingowe.Length];
                    for (int j = 0; j < miejscaParkingowe.Length; j++)
                    {
                        daneZPliku[i][j] = int.Parse(miejscaParkingowe[j]);

                    }
                }

                // FISHER

                List<int> decyzje = new List<int>();

                Dictionary<int, int> decyzjaIIlosc = new Dictionary<int, int>();

                Dictionary<int, List<int>> klasyObliczone = new Dictionary<int, List<int>>();

                
                foreach (var item in daneZPliku)
                {
                    if (decyzjaIIlosc.ContainsKey(item.Last()))
                    {
                        decyzjaIIlosc[item.Last()] += 1;
                    }
                    else
                        decyzjaIIlosc.Add(item.Last(), +1);                    
                }

                // obliczamy dla atrybutów
                List<double> obliczoneSrednie = new List<double>();
                Dictionary<int, List<double>> sredniaDlaKlasy = new Dictionary<int, List<double>>();
                Dictionary<int, List<double>> sredniaDlaKlasyReszta = new Dictionary<int, List<double>>();

                Dictionary<int, Dictionary<int, double>> separacjaKlas = new Dictionary<int, Dictionary<int, double>>();

                foreach (var klasa in decyzjaIIlosc) // klasy
                {

                    for (int a = 0; a < daneZPliku[0].Length - 1; a++) // atrybuty
                    {

                        List<int> listaZKlasa = new List<int>(), listaReszta = new List<int>();
                        double klasaSrednia = 0, resztaSrednia = 0, wynikSrednia = 0, wynikResztaSrednia = 0, wynikSeparacjaKlas = 0, suma = 0, sumaReszty = 0;
                        for (int i = 0; i < daneZPliku.Length; i++) // obliczamy sume dla danego atrybutu z dana klasa
                            {
                                if (daneZPliku[i].Last() == klasa.Key)
                                { 
                                    suma += daneZPliku[i][a];
                                    listaZKlasa.Add(daneZPliku[i][a]);
                                }
                                else
                                {
                                    sumaReszty += daneZPliku[i][a];
                                    listaReszta.Add(daneZPliku[i][a]);
                                }
                                
                            }
                            klasaSrednia = (double)(suma / decyzjaIIlosc[klasa.Key]);
                            resztaSrednia = (double)(sumaReszty / (daneZPliku.Length - decyzjaIIlosc[klasa.Key]));
                        foreach (var item in listaZKlasa)
                        {
                            wynikSrednia += Math.Pow(item - klasaSrednia, 2);
                        }
                        wynikSrednia /= decyzjaIIlosc[klasa.Key];

                        foreach (var item in listaReszta)
                        {
                            wynikResztaSrednia += Math.Pow(item - resztaSrednia, 2);
                        }
                        wynikResztaSrednia /= (daneZPliku.Length - decyzjaIIlosc[klasa.Key]);

                        wynikSeparacjaKlas = (Math.Pow(klasaSrednia - resztaSrednia, 2)) / (wynikSrednia + wynikResztaSrednia);

                        if (!separacjaKlas.ContainsKey(klasa.Key))
                            separacjaKlas.Add(klasa.Key, new Dictionary<int, double>());
                        
                        separacjaKlas[klasa.Key].Add(a, wynikSeparacjaKlas);

                        //
                    }
                    separacjaKlas[klasa.Key] = separacjaKlas[klasa.Key].OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                }
                IEnumerable<int> listaIndeksow = wybierzNajlepszeAtrybuty(separacjaKlas, Convert.ToInt32(numericUpDown1.Value));

                foreach (var atrybut in listaIndeksow)
                {
                    textBoxFisher.Text +=  "a(" + Convert.ToInt32(atrybut + 1) + ") ";
                }

                textBoxFisher.Text += "d";
                textBoxFisher.AppendText(Environment.NewLine);

                foreach (var i in daneZPliku)
                {
                    foreach (var j in listaIndeksow)
                    {
                        textBoxFisher.Text += i[j] + " ";
                    }
                    textBoxFisher.Text += " " + i.Last();
                    textBoxFisher.AppendText(Environment.NewLine);
                }
            }

        }


        IEnumerable<int> wybierzNajlepszeAtrybuty (Dictionary<int, Dictionary<int, double>> najlepszeAtrybuty, int ileAtrybutow){

            

            List<int> listaIndeksowNajlepszychAtrybutow = new List<int>();
            var liczbaArgumentow = najlepszeAtrybuty.First().Value.Keys.LongCount();

            do
            {
                foreach (var i in najlepszeAtrybuty)
                {
                    foreach (var j in i.Value) // slownik wewnetrzny
                    {
                        
                        if (!listaIndeksowNajlepszychAtrybutow.Contains(j.Key))
                        {
                            listaIndeksowNajlepszychAtrybutow.Add(j.Key);
                            i.Value.Remove(j.Key);
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            } while (listaIndeksowNajlepszychAtrybutow.LongCount() != liczbaArgumentow);
            //.Take(3)

            IEnumerable<int> lista = listaIndeksowNajlepszychAtrybutow.Take(ileAtrybutow);
            return lista;
            
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ofd.Filter = "Text Filrd (.txt) |*.txt";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            btnWybierz_Click(sender, e);
            numericUpDown1.Maximum = daneZPliku[0].Length - 1;
            numericUpDown1.Minimum = 1;
        }
    }
}
