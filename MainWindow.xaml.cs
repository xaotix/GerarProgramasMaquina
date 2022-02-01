using Conexoes;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GerarProgramasMaquina
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Conexoes.Utilz.SetIcones(this.menu);
            this.Title = System.Windows.Forms.Application.ProductName + " - V" + System.Windows.Forms.Application.ProductVersion;
        }

        public List<string> Arquivos
        {
            get
            {
                return this.programas.Select(x => x.Arquivo).Distinct().ToList();
            }
        }

        public List<Conexoes.Medajoist_Programa> programas { get; set; } = new List<Conexoes.Medajoist_Programa>();

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.lista.ItemsSource = null;
            this.programas.Clear();
        }

        private void iMPORTAR(string ss)
        {
            if (ss != null)
            {
                var ps = Conexoes.Utilz.Medajoist.GetProgramas(ss);

                if (ps.Count == 0)
                {
                    Conexoes.Utilz.Alerta("Nenhum programa de máquina encontrado no arquivo " + ss);
                    return;
                }
                var jaexiste = ps.FindAll(x => programas.Find(y => y.NomeOriginal == x.NomeOriginal) != null);
                var naoexiste = ps.FindAll(x => programas.Find(y => y.NomeOriginal == x.NomeOriginal) == null);
                //programas = new List<Conexoes.Medajoist_Programa>();
                if (this.programas.Find(x => x.Arquivo.ToUpper().Replace(" ", "") == ps.First().Arquivo.ToUpper().Replace(" ", "")) != null)
                {
                    programas.AddRange(naoexiste);
                }
                else
                if (jaexiste.Count == 0)
                {
                    programas.AddRange(ps);
                }
                else if (jaexiste.Count > 0 && naoexiste.Count > 0)
                {
                    if (Conexoes.Utilz.Pergunta("Já existem " + jaexiste.Count + " programas com o nome igual na lista de outro arquivo que foi importado:\n" + string.Join("\n", jaexiste.Select(x => x.NomeOriginal)) + "\nDeseja ignorá - los e importar os demais ? "))
                    {
                        programas.AddRange(naoexiste);
                    }
                    else
                    {
                        return;
                    }
                }
                this.programas = programas.OrderBy(x => x.NomeOriginal).ToList();
                this.lista.ItemsSource = null;
                this.lista.ItemsSource = programas;
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var ss = Conexoes.Utilz.AbrirArquivos("Selecione", new List<string> { "b*d.mfg" });
            foreach (var s in ss)
            {
                iMPORTAR(s);
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            var sel = this.lista.SelectedItems.Cast<Conexoes.Medajoist_Programa>().ToList();
            if (sel.Count == 0)
            {
                if (Conexoes.Utilz.Pergunta("Não há nenhum banzo selecionado na lsita. Deseja selecionar todos?"))
                {
                    sel = this.programas;
                }
            }
            else if (sel.Count != this.programas.Count)
            {
                if (!Conexoes.Utilz.Pergunta("Deseja exportar somente os itens selecionados?\nSe clicar em não, toda a lista será exportada."))
                {
                    sel = this.programas;
                }
            }

            List<Medajoist_Programa> erross;
            var erros = Conexoes.Utilz.Medajoist.VerificarBanzos(sel, out erross);
            if (erros.Count > 0)
            {
                if (!Conexoes.Utilz.Pergunta("Foram encontradas divergências. Deseja gerar mesmo assim?"))
                {
                    goto ss;
                }
                Conexoes.Utilz.ShowReports(erros);
                return;
            }
        ss:
            var pdf = Conexoes.Utilz.SalvarArquivo("PDF");
            if (pdf != "")
            {
                Conexoes.Classes.PDF pp = new Conexoes.Classes.PDF();
                pp.Report(sel.OrderBy(x => x.NomeOriginal).ToList(), pdf);

                Conexoes.Utilz.Abrir(pdf);
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Conexoes.Utilz.Medajoist.SalvarDXFBanzos(this.programas);
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            Conexoes.Utilz.JanelaTexto("Suporte:" +
                "\n Daniel Lins Maciel" +
                "\n054-3273-4010" +
                "\ndaniel.maciel@medabil.com.br" +
                "\nInovação - 2021" +
                "\nVersões:" +
                "\nv1.0 - Versão inicial" +
                "\nv1.1 - Corrigido bug em nós variáveis" +
                "\nv1.2 - Removido indicação de furo a 18 em nós que tem furação variável em algum lado." +
                "\nv1.3 - Ajuste coordenadas"+
                "\nv1.4 - Validação de grupos" + 
                "\nv1.5 - Ajuste interface" +
                "\nv1.6 - Melhorias nas verificações" +
                "\nv1.7 - Remoção automática de furos sobrepostos com distância menor que 3 mm"
                );
        }

        private void verificar(object sender, RoutedEventArgs e)
        {
            var sel = this.lista.SelectedItems.Cast<Conexoes.Medajoist_Programa>().ToList();
            if (sel.Count == 0)
            {
                if (Conexoes.Utilz.Pergunta("Não há nenhum banzo selecionado na lsita. Deseja selecionar todos?"))
                {
                    sel = this.programas;
                }
            }
            else if (!Conexoes.Utilz.Pergunta("Deseja exportar somente os itens selecionados?\nSe clicar em não, toda a lista será exportada."))
            {
                sel = this.programas;
            }

            List<Medajoist_Programa> erross;
            var erros = Conexoes.Utilz.Medajoist.VerificarBanzos(sel, out erross);
            if (erros.Count > 0)
            {
                Conexoes.Utilz.ShowReports(erros);
            }
            else
            {
                MessageBox.Show("Nenhum erro encontrado.");
            }
        }
    }
}