using ARS.WinForms.Classes;
using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using ST = System.Timers;

namespace ARS.WinForms
{

	public interface ICustomControlsARS
	{
		bool IsValid { get; set; }
		bool IsEmpty { get; }
		bool IsRequired { get; set; }
		Label RequiredFieldLabel { set; }
		CultureInfo Culture { get; set; }
	}

	public interface ICustomLabelARS
	{
		bool TextToEmptyAfterTimeoutEnabled { set; }
		double TimeoutToTextEmpty { get; set; }
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
		public bool TextToEmptyAfterTimeoutEnabled { get; set; } = true;
		public double TimeoutToTextEmpty { get; set; } = 2000;

		protected override void OnTextChanged(EventArgs e)
		{
			if (TextToEmptyAfterTimeoutEnabled)
				TimerManager.Instance.LimpaMensagemAviso(this, TimeoutToTextEmpty);

			base.OnTextChanged(e);
		}
		 
	}

	public class ARSToolStripLabel : ToolStripLabel, ICustomLabelARS
	{
		public bool TextToEmptyAfterTimeoutEnabled { get; set; } = true;
		public double TimeoutToTextEmpty { get; set; } = 2000;

		private ST.Timer _timer;

		protected override void OnTextChanged(EventArgs e)
		{
			if (TextToEmptyAfterTimeoutEnabled)
				StartClearTextTimer();

			base.OnTextChanged(e);
		}

		private void StartClearTextTimer()
		{
			if (_timer != null)
			{
				_timer.Stop();
				_timer.Dispose();
			}

			_timer = new ST.Timer(TimeoutToTextEmpty)
			{
				AutoReset = false
			};

			_timer.Elapsed += (sender, args) =>
			{
				_timer.Stop();
				_timer.Dispose();
				_timer = null;

				if (Owner?.InvokeRequired == false)
					Text = string.Empty;
				else
					Owner?.BeginInvoke(new Action(() => Text = string.Empty));
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

		public bool IsValid
		{
			get => !(IsRequired && string.IsNullOrEmpty(Text));

			set
			{
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

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			IsValid = !(IsRequired && string.IsNullOrEmpty(Text)); 
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

				double value;
				var success = double.TryParse(Text, out value);
				Thread.CurrentThread.CurrentCulture = Culture;

				return success ? value : double.MinValue;
			}
			set => Text = value != double.MinValue ? value.ToString() : string.Empty;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (Text == "," || Text == ".")
			{
				base.OnTextChanged(e);
				return;
			}

			double result;
			if (double.TryParse(Text, out result) || string.IsNullOrEmpty(Text))
			{
				_formerValue = Text;
				ForeColor = SystemColors.WindowText;
			}
			else
			{
				Text = _formerValue;
				ForeColor = Color.Red;
				SelectionStart = Text.Length + 1;
			}

			base.OnTextChanged(e);
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
				int value;
				return int.TryParse(Text, out value) ? value : int.MinValue;
			}
			set => Text = value != int.MinValue ? value.ToString() : string.Empty;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			int result;
			if (int.TryParse(Text, out result) || string.IsNullOrEmpty(Text))
			{
				_formerValue = Text;
				ForeColor = SystemColors.WindowText;
			}
			else
			{
				Text = _formerValue;
				ForeColor = Color.Red;
				SelectionStart = Text.Length + 1;
			}

			IsValid = !(IsRequired && string.IsNullOrEmpty(Text));
			base.OnTextChanged(e);
		}
	}

	public class EmailTextBox : ARSTextBox
	{
		private string _formerValue = string.Empty;

		protected override void OnLeave(EventArgs e)
		{
			if (Util.IsEmail(Text) || string.IsNullOrEmpty(Text))
			{
				ForeColor = SystemColors.WindowText;
				_formerValue = Text;
			}
			else
			{
				ForeColor = Color.Red;
				Text = _formerValue;
				SelectionStart = Text.Length + 1;
			}

			base.OnLeave(e);
		}
	}


	public class CEPTextBox : ARSTextBox
	{
		protected override void OnLostFocus(EventArgs e)
		{
			IsValid = Util.IsCep(Text);
			base.OnLostFocus(e);
			if (IsValid)
				IsValid = !(IsRequired && string.IsNullOrEmpty(Text));
		}
	}

	public class CPFTextBox : ARSTextBox
	{
		protected override void OnLostFocus(EventArgs e)
		{
			IsValid = Util.IsCPF(Text);
			base.OnLostFocus(e);
			if (IsValid)
				IsValid = !(IsRequired && string.IsNullOrEmpty(Text));
		}
	}

	public class CNPJTextBox : ARSTextBox
	{
		protected override void OnLostFocus(EventArgs e)
		{
			IsValid = Util.IsCNPJ(Text);
			base.OnLostFocus(e);
			if (IsValid)
				IsValid = !(IsRequired && string.IsNullOrEmpty(Text));
		}
	}


	#endregion



}
