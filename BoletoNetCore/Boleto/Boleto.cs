using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BoletoNetCore
{
    [Serializable]
    [Browsable(false)]
    public class Boleto
    {
        /// <summary>
        /// Construtor da Classe Boleto
        /// </summary>
        /// <param name="banco"></param>
        public Boleto(IBanco banco)
        {
            Banco = banco;
            Carteira = banco.Cedente.ContaBancaria.CarteiraPadrao;
            CarteiraImpressaoBoleto = banco.Cedente.ContaBancaria.CarteiraPadrao;
            VariacaoCarteira = banco.Cedente.ContaBancaria.VariacaoCarteiraPadrao;
            TipoCarteira = banco.Cedente.ContaBancaria.TipoCarteiraPadrao;
        }

        /// <summary>
        /// Construtor da Classe Boleto com par�metro para viabilizar v�rias carteiras
        /// </summary>
        /// <param name="banco"></param>
        /// <param name="ignorarCarteira"></param>
        public Boleto(IBanco banco, Boolean ignorarCarteira)
        {
            Banco = banco;
            //se o arquivo de retorno for criado par multiplas carteiras, ignora a carteira (para compatibilidade)
            if (!ignorarCarteira)
            {
                Carteira = banco.Cedente.ContaBancaria.CarteiraPadrao;
                VariacaoCarteira = banco.Cedente.ContaBancaria.VariacaoCarteiraPadrao;
                TipoCarteira = banco.Cedente.ContaBancaria.TipoCarteiraPadrao;
            }
        }

        public int CodigoMoeda { get; set; } = 9;
        public string EspecieMoeda { get; set; } = "R$";
        public int QuantidadeMoeda { get; set; } = 0;
        public string ValorMoeda { get; set; } = string.Empty;

        public TipoEspecieDocumento EspecieDocumento { get; set; } = TipoEspecieDocumento.NaoDefinido;

        public string NossoNumero { get; set; } = string.Empty;
        public string NossoNumeroDV { get; set; } = string.Empty;
        public string NossoNumeroFormatado { get; set; } = string.Empty;

        public TipoCarteira TipoCarteira { get; set; } = TipoCarteira.CarteiraCobrancaSimples;
        public string Carteira { get; set; } = string.Empty;
        public string VariacaoCarteira { get; set; } = string.Empty;
        public string CarteiraComVariacao => string.IsNullOrEmpty(Carteira) || string.IsNullOrEmpty(VariacaoCarteira) ? $"{Carteira}{VariacaoCarteira}" : $"{Carteira}/{VariacaoCarteira}";
        public string CarteiraImpressaoBoleto { get; set; } = string.Empty;

        public DateTime DataProcessamento { get; set; } = DateTime.Now;
        public DateTime DataEmissao { get; set; } = DateTime.Now;
        public DateTime DataVencimento { get; set; }
        public DateTime DataCredito { get; set; }

        public string NumeroDocumento { get; set; } = string.Empty;
        public string NumeroControleParticipante { get; set; } = string.Empty;
        public string Aceite { get; set; } = "N";
        public string UsoBanco { get; set; } = string.Empty;

        // Valores do Boleto
        public decimal ValorTitulo { get; set; }

        public bool ImprimirValoresAuxiliares { get; set; } = false;
        public decimal ValorPago { get; set; } // ValorPago deve ser preenchido com o valor que o sacado pagou. Se n�o existir essa informa��o no arquivo retorno, deixar zerada.
        public decimal ValorPagoCredito { get; set; } // ValorPagoCredito deve ser preenchido com o valor que ser� creditado na conta corrente. Se n�o existir essa informa��o no arquivo retorno, deixar zerada.
        public decimal ValorJurosDia { get; set; }
        public decimal ValorMulta { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorTarifas { get; set; }
        public decimal ValorOutrasDespesas { get; set; }
        public decimal ValorOutrosCreditos { get; set; }
        public decimal ValorIOF { get; set; }
        public decimal ValorAbatimento { get; set; }

        // Juros
        public decimal PercentualJurosDia { get; set; }

        public DateTime DataJuros { get; set; }

        // Multa
        public decimal PercentualMulta { get; set; }

        public DateTime DataMulta { get; set; }

        // Desconto
        public DateTime DataDesconto { get; set; }

        /// <summary>
        /// Banco no qual o boleto/t�tulo foi quitado/recolhido
        /// </summary>
        public string BancoCobradorRecebedor { get; set; }
        
        /// <summary>
        /// Ag�ncia na qual o boleto/t�tulo foi quitado/recolhido
        /// </summary>
        public string AgenciaCobradoraRecebedora { get; set; }

        /// <summary>
        /// C044 - C�digo de Movimento Retorno
        /// C�digo adotado pela FEBRABAN, para identificar o tipo de movimenta��o enviado nos
        /// registros do arquivo de retorno.
        public string CodigoMovimentoRetorno { get; set; } = "01";

        /// <summary>
        /// C044 - Descri��o do Movimento Retorno
        /// Descri��o do C�digo adotado pela FEBRABAN, para identificar o tipo de movimenta��o enviado nos
        /// registros do arquivo de retorno. 
        /// </summary>
        public string DescricaoMovimentoRetorno { get; set; } = string.Empty;

        /// <summary>
        /// C047 - Motivo da Ocorr�ncia
        /// C�digo adotado pela FEBRABAN para identificar as ocorr�ncias (rejei��es, tarifas,
        /// custas, liquida��o e baixas) em registros detalhe de t�tulos de cobran�a.Poder�o ser
        /// informados at� cinco ocorr�ncias distintas, incidente sobre o t�tulo.
        /// </summary>
        public string CodigoMotivoOcorrencia { get; set; } = string.Empty;

        /// <summary>
        /// C047 - Descri��o do Motivo da Ocorr�ncia
        /// Descri��o do C�digo adotado pela FEBRABAN para identificar as ocorr�ncias (rejei��es, tarifas,
        /// custas, liquida��o e baixas) em registros detalhe de t�tulos de cobran�a.Poder�o ser
        /// informados at� cinco ocorr�ncias distintas, incidente sobre o t�tulo.
        /// </summary>
        public string DescricaoMotivoOcorrencia { get => string.Join(", ", ListMotivosOcorrencia); }

        /// <summary>
        /// C047 - Descri��o do Motivo da Ocorr�ncia
        /// Descri��o do C�digo adotado pela FEBRABAN para identificar as ocorr�ncias (rejei��es, tarifas,
        /// custas, liquida��o e baixas) em registros detalhe de t�tulos de cobran�a.Poder�o ser
        /// informados at� cinco ocorr�ncias distintas, incidente sobre o t�tulo.
        /// </summary>
        public IEnumerable<string> ListMotivosOcorrencia { get; set; } = Enumerable.Empty<string>();

        public TipoCodigoProtesto CodigoProtesto { get; set; } = TipoCodigoProtesto.NaoProtestar;
        public int DiasProtesto { get; set; } = 0;
        public TipoCodigoBaixaDevolucao CodigoBaixaDevolucao { get; set; } = TipoCodigoBaixaDevolucao.BaixarDevolver;
        public int DiasBaixaDevolucao { get; set; } = 60;

        public string CodigoInstrucao1 { get; set; } = string.Empty;
        public string ComplementoInstrucao1 { get; set; } = string.Empty;
        public string CodigoInstrucao2 { get; set; } = string.Empty;
        public string ComplementoInstrucao2 { get; set; } = string.Empty;
        public string CodigoInstrucao3 { get; set; } = string.Empty;
        public string ComplementoInstrucao3 { get; set; } = string.Empty;

        public string MensagemInstrucoesCaixa { get; set; } = string.Empty;
        public string MensagemInstrucoesCaixaFormatado { get; set; } = string.Empty;
        public string MensagemArquivoRemessa { get; set; } = string.Empty;
        public string RegistroArquivoRetorno { get; set; } = string.Empty;

        public IBanco Banco { get; set; }
        public Sacado Sacado { get; set; } = new Sacado();
        public Sacado Avalista { get; set; } = new Sacado();
        public CodigoBarra CodigoBarra { get; } = new CodigoBarra();
        public ObservableCollection<GrupoDemonstrativo> Demonstrativos { get; } = new ObservableCollection<GrupoDemonstrativo>();

        public void ValidarDados()
        {
            // Banco Obrigat�rio
            if (Banco == null)
                throw new Exception("Boleto n�o possui Banco.");

            // Cedente Obrigat�rio
            if (Banco.Cedente == null)
                throw new Exception("Boleto n�o possui cedente.");

            // Conta Banc�ria Obrigat�ria
            if (Banco.Cedente.ContaBancaria == null)
                throw new Exception("Boleto n�o possui conta banc�ria.");

            // Sacado Obrigat�rio
            if (Sacado == null)
                throw new Exception("Boleto n�o possui sacado.");

            // Verifica se data do processamento � valida
            if (DataProcessamento == DateTime.MinValue)
                DataProcessamento = DateTime.Now;

            // Verifica se data de emiss�o � valida
            if (DataEmissao == DateTime.MinValue)
                DataEmissao = DateTime.Now;

            // Aceite
            if ((Aceite != "A") & (Aceite != "N") & (Aceite != "S"))
                throw new Exception("Aceite do Boleto deve ser definido com A, S ou N");

            Banco.ValidaBoleto(this);
            Banco.FormataNossoNumero(this);
            BoletoNetCore.Banco.FormataCodigoBarra(this);
            BoletoNetCore.Banco.FormataLinhaDigitavel(this);
            BoletoNetCore.Banco.FormataMensagemInstrucao(this);

        }
    }
}