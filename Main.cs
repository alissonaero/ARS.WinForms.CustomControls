using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace ARS.WinForms
{

	public interface IDocumentField
	{
		bool ApplyMaskOnFocusLeave { get; set; }
	}


	public interface ICustomControlsARS
	{

		bool IsValid { get; set; }
		bool IsEmpty { get; }
		bool IsRequired { get; set; }
		Label RequiredFieldLabel { set; }
		CultureInfo Culture { get; set; }
		bool ValidateInput();
	}

	public interface ICustomLabelARS
	{
		bool RestoreInitialTextAfterTimeout { set; }

		double TextTimeout { get; set; }
	}

	public interface INumericRangeControlARS
	{
		double MinValue { get; set; }
		double MaxValue { get; set; }
	}

	#region Labels

	/// <summary>
	/// Para usar, deixe o texto como vazio. Ou deixe com texto para deixar o controle visivel no form durante edição e set como vazio durante o formload.
	/// TextToEmptyAfterTimeoutEnabled = True para o texto sumir segundos após eceber algum valor.
	/// </summary>
	public class ARSLabel : Label, ICustomLabelARS
	{
		private System.Timers.Timer _timer;

		public string InitialText { get; set; } = string.Empty;

		public bool RestoreInitialTextAfterTimeout { get; set; } = false;

		public bool TemporaryVisibility { get; set; } = false;

		public double TextTimeout { get; set; } = 3000;

		private Color? _initialForeColor = null;

		protected override void OnCreateControl()
		{
			base.OnCreateControl();

			// Se for temporário, começa invisível
			if (TemporaryVisibility)
			{
				this.Visible = false;
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			// Se ainda não definimos o texto inicial
			if (string.IsNullOrEmpty(InitialText))
				InitialText = Text;

			// Salva a cor inicial do controle
			if (_initialForeColor == null)
				_initialForeColor = this.ForeColor;

			// Se for temporário, torna visível e agenda para sumir
			if (TemporaryVisibility)
			{
				this.Visible = true;
				StartTimer(() => this.Visible = !this.Visible);
			}

			// Se for label com texto temporário que deve voltar ao original
			else if (RestoreInitialTextAfterTimeout && Text != InitialText)
			{
				StartTimer(() =>
				{
					this.Text = InitialText;
					if (_initialForeColor.HasValue)
						this.ForeColor = _initialForeColor.Value;
				});
			}
		}

		private void StartTimer(Action callback)
		{
			_timer?.Stop();
			_timer?.Dispose();

			_timer = new System.Timers.Timer(TextTimeout)
			{
				AutoReset = false,
				SynchronizingObject = this
			};

			_timer.Elapsed += (s, e) =>
			{
				if (!this.IsDisposed && this.IsHandleCreated)
				{
					callback?.Invoke();
				}
			};

			_timer.Start();
		}
	}


	public class ARSToolStripLabel : ToolStripLabel, ICustomLabelARS
	{
		private System.Timers.Timer _timer;

		public string InitialText { get; set; } = string.Empty;

		public bool RestoreInitialTextAfterTimeout { get; set; } = false;

		public bool TemporaryVisibility { get; set; } = false;

		public double TextTimeout { get; set; } = 3000;

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			if (string.IsNullOrEmpty(InitialText))
				InitialText = Text;

			// Se for uso temporário: mostra e esconde
			if (TemporaryVisibility)
			{
				this.Visible = true;
				StartTimer(() => this.Visible = false);
			}
			// Se deve restaurar texto inicial após tempo
			else if (RestoreInitialTextAfterTimeout && Text != InitialText)
			{
				StartTimer(() => this.Text = InitialText);
			}
		}

		private void StartTimer(Action callback)
		{
			_timer?.Stop();
			_timer?.Dispose();

			_timer = new System.Timers.Timer(TextTimeout)
			{
				AutoReset = false,
				SynchronizingObject = this.Owner
			};

			_timer.Elapsed += (s, e) =>
			{
				if (Owner != null && !Owner.IsDisposed)
				{
					Owner.BeginInvoke(new Action(() => callback?.Invoke()));
				}
			};

			_timer.Start();
		}
	}




	#endregion

	#region TextBoxes

	public class ARSTextBox : TextBox, ICustomControlsARS
	{
		private Label _requiredFieldLabel;

		private CultureInfo _culture = new CultureInfo(127);

		public Label ErrorMessageLabel { get; set; }

		private bool _isValid = true;

		public bool IsValid
		{
			get
			{
				_isValid = ValidateInput();

				return _isValid;
			}

			set
			{
				_isValid = value;

				if (_requiredFieldLabel != null)
					_requiredFieldLabel.ForeColor = value ? SystemColors.ControlText : Color.Red;
			}
		}

		public bool IsEmpty => string.IsNullOrEmpty(Text);

		public bool IsRequired { get; set; }

		public Label RequiredFieldLabel
		{
			set => _requiredFieldLabel = value;
		}

		public CultureInfo Culture
		{
			get => _culture;
			set => _culture = value;
		}

		private string _placeholderText;
		private Color _placeholderColor = Color.Gray;
		private Color _defaultForeColor;

		public void SetPlaceholder(string placeholder)
		{
			_placeholderText = placeholder;
			_defaultForeColor = ForeColor;

			if (string.IsNullOrEmpty(Text))
			{
				Text = _placeholderText;
				ForeColor = _placeholderColor;
			}

			GotFocus += RemovePlaceholder;
			LostFocus += ApplyPlaceholder;
		}

		private void RemovePlaceholder(object sender, EventArgs e)
		{
			if (Text == _placeholderText)
			{
				Text = string.Empty;
				ForeColor = _defaultForeColor;
			}
		}

		private void ApplyPlaceholder(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Text))
			{
				Text = _placeholderText;
				ForeColor = _placeholderColor;
			}
		}

		public virtual bool ValidateInput()
		{
			return !(IsRequired && string.IsNullOrEmpty(Text));
		}

		protected override void OnLostFocus(EventArgs e)
		{
			IsValid = ValidateInput();
			base.OnLostFocus(e);
		}

	}

	public class DoubleTextBox : ARSTextBox, INumericRangeControlARS
	{
		private string _formerValue = string.Empty;

		public double MinValue { get; set; } = double.MinValue;
		public double MaxValue { get; set; } = double.MaxValue;

		public double TypedValue
		{
			get
			{
				if (Text.Contains(","))
					Thread.CurrentThread.CurrentCulture = new CultureInfo(1046);

				var success = double.TryParse(Text, out double value);
				Thread.CurrentThread.CurrentCulture = Culture;

				return success ? value : double.MinValue;
			}
			set => Text = value != double.MinValue ? value.ToString() : string.Empty;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (string.IsNullOrEmpty(Text) || Text == "," || Text == ".")
			{
				base.OnTextChanged(e);
				return;
			}

			if (!double.TryParse(Text, out _))
			{
				Text = _formerValue;
				ForeColor = Color.Red;
				SelectionStart = Text.Length + 1;

				base.OnTextChanged(e);
				return;
			}

			_formerValue = Text;
			ForeColor = SystemColors.WindowText;
			base.OnTextChanged(e);
		}
	}

	public class ARSCurrencyTextBox : ARSTextBox, INumericRangeControlARS
	{
		private string _formerValue = string.Empty;

		public double MinValue { get; set; } = 0;
		public double MaxValue { get; set; } = double.MaxValue;

		public double TypedValue
		{
			get
			{
				return double.TryParse(Text, NumberStyles.Currency, Culture, out double value) ? value : double.MinValue;
			}
			set => Text = value != double.MinValue ? value.ToString("C", Culture) : string.Empty;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (string.IsNullOrEmpty(Text) || Text == "," || Text == ".")
			{
				base.OnTextChanged(e);
				return;
			}

			if (!double.TryParse(Text, NumberStyles.Currency, Culture, out _))
			{
				Text = _formerValue;
				ForeColor = Color.Red;
				SelectionStart = Text.Length + 1;
				base.OnTextChanged(e);
				return;
			}

			_formerValue = Text;
			ForeColor = SystemColors.WindowText;
			base.OnTextChanged(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			if (double.TryParse(Text, NumberStyles.Currency, Culture, out var value))
				Text = value.ToString("C", Culture);
		}
	}


	public class IntegerTextBox : ARSTextBox, INumericRangeControlARS
	{
		private string _formerValue = string.Empty;

		public double MinValue { get; set; } = int.MinValue;
		public double MaxValue { get; set; } = int.MaxValue;

		public int TypedValue
		{
			get
			{
				return int.TryParse(Text, out int value) ? value : int.MinValue;
			}
			set => Text = value != int.MinValue ? value.ToString() : string.Empty;
		}

		protected override void OnTextChanged(EventArgs e)
		{

			if (string.IsNullOrEmpty(Text))
			{
				base.OnTextChanged(e);
				return;
			}

			if (!int.TryParse(Text, out _))
			{
				Text = _formerValue;
				ForeColor = Color.Red;
				SelectionStart = Text.Length + 1;
				base.OnTextChanged(e);
				return;
			}

			_formerValue = Text;

			ForeColor = SystemColors.WindowText;

			IsValid = !(IsRequired && string.IsNullOrEmpty(Text));

			base.OnTextChanged(e);
		}
	}

	public class EmailTextBox : ARSTextBox
	{
		private string _formerValue = string.Empty;

		protected override void OnLeave(EventArgs e)
		{

			if (string.IsNullOrEmpty(Text))
			{
				ForeColor = SystemColors.WindowText;
				_formerValue = Text;
				base.OnLeave(e);
				return;
			}

			if (!Util.IsEmail(Text))
			{
				ForeColor = Color.Red;
				Text = _formerValue;
				SelectionStart = Text.Length + 1;
				base.OnLeave(e);
				return;
			}

			ForeColor = SystemColors.WindowText;
			_formerValue = Text;

			base.OnLeave(e);
		}
	}


	public class CEPTextBox : ARSTextBox, IDocumentField
	{
		public bool ApplyMaskOnFocusLeave { get; set; } = true;


		public override int MaxLength
		{
			get { return base.MaxLength; }
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			//Em caso de redigitação pos validação exitosa, o Maxlength estará em 9 (a máscara foi aplicada).
			//Sendo assim, toda vez que o mouse entrar no controle, devemos conferir se o usuário está digitando novamente e se sim, limitar o MaxLength para o default do CEP
			if (base.MaxLength == 9)
				base.MaxLength = 8;
 
			base.OnMouseEnter(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			IsValid = ValidateInput();

		
			

			base.OnLostFocus(e);
		}

		public override bool ValidateInput()
		{

			// Se o campo é obrigatório e está vazio, returna falso
			if (IsRequired && string.IsNullOrWhiteSpace(Text))
			{
				IsValid = false;
				return false;
			}

			/*Passou aqui, pode ou não ser requerido, mas não pode ser inválido*/

			//Se for vazio ou em forto inválido, returna falso
			if (!Util.IsCep(Text))
			{
				return false;
			}

			var rawCep = Text.Replace("-", "").Trim() ?? string.Empty;

			if (ApplyMaskOnFocusLeave && rawCep.Length == 8)
			{
				Text = rawCep.Insert(5, "-");

				base.MaxLength = 9;
			}


			return true;
		}

	}

	public class CPFTextBox : ARSTextBox
	{
		protected override void OnLostFocus(EventArgs e)
		{
			IsValid = Util.IsCPF(Text);

			base.OnLostFocus(e);

			if (IsValid && IsRequired)
			{
				IsValid = !string.IsNullOrWhiteSpace(Text);
			}
		}
	}

	public class CNPJTextBox : ARSTextBox
	{
		protected override void OnLostFocus(EventArgs e)
		{
			IsValid = Util.IsCNPJ(Text);
			base.OnLostFocus(e);

			if (IsValid && IsRequired)
			{
				IsValid = !string.IsNullOrWhiteSpace(Text);
			}
		}
	}


	#endregion



}
