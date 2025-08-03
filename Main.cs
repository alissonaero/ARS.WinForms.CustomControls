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
	/// Represents a specialized label control with support for temporary text visibility,  automatic text restoration, and
	/// timeout-based behavior.
	/// </summary>
	/// <remarks>The <see cref="ARSLabel"/> class extends the standard <see cref="Label"/> control to provide 
	/// additional functionality, including: <list type="bullet"> <item>Temporary visibility for labels, which
	/// automatically toggles after a specified timeout.</item> <item>Automatic restoration of the initial text after a
	/// timeout when the text is changed.</item> <item>Customizable timeout duration for text restoration or visibility
	/// toggling.</item> </list> This control is useful for scenarios where labels need to display temporary messages or 
	/// dynamically change visibility based on user interaction or application state.</remarks>
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

	/// <summary>
	/// Represents a specialized <see cref="ToolStripLabel"/> with additional functionality for temporary text visibility, 
	/// automatic text restoration, and timeout-based behavior.
	/// </summary>
	/// <remarks>The <see cref="ARSToolStripLabel"/> provides features such as: <list type="bullet">
	/// <item><description>Temporary visibility of the label, controlled by the <see cref="TemporaryVisibility"/>
	/// property.</description></item> <item><description>Automatic restoration of the initial text after a specified
	/// timeout, controlled by the <see cref="RestoreInitialTextAfterTimeout"/> property.</description></item>
	/// <item><description>Customizable timeout duration for text restoration or visibility, specified by the <see
	/// cref="TextTimeout"/> property.</description></item> </list> This class is useful for scenarios where dynamic text
	/// updates or temporary visibility are required, such as status messages or notifications.</remarks>
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

	/// <summary>
	/// Represents a custom text box control with additional functionality for validation, placeholder text,  and required
	/// field indication.
	/// </summary>
	/// <remarks>The <see cref="ARSTextBox"/> class extends the standard <see cref="TextBox"/> control to provide 
	/// features such as input validation, placeholder text management, and required field handling.  It is designed to
	/// simplify form input scenarios where validation and user guidance are needed.</remarks>
	public class ARSTextBox : TextBox, ICustomControlsARS
	{
		 
		private Label _requiredFieldLabel;

		private CultureInfo _culture = new CultureInfo(127);

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

				
			}
		}

		public bool IsEmpty => string.IsNullOrEmpty(Text);

		public bool IsRequired { get; set; }

		public Label RequiredFieldLabel
		{
			set => _requiredFieldLabel = value;
			get => _requiredFieldLabel;
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

			if (_requiredFieldLabel != null)
			{
				_requiredFieldLabel.ForeColor = (IsValid ? SystemColors.ControlText : Color.Red);
			}

			base.OnLostFocus(e);
		}

	}

	/// <summary>
	/// Represents a text box control for inputting and displaying double-precision numeric values.
	/// </summary>
	/// <remarks>The <see cref="DoubleTextBox"/> allows users to input numeric values within a specified range,
	/// defined by  <see cref="MinValue"/> and <see cref="MaxValue"/>. It validates the input to ensure it is a valid
	/// double-precision  number and provides feedback by changing the text color to red for invalid input.  The <see
	/// cref="TypedValue"/> property provides direct access to the numeric value represented by the text box, 
	/// automatically parsing the text input. If the input is invalid, <see cref="TypedValue"/> returns <see
	/// cref="double.MinValue"/>.</remarks>
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

	/// <summary>
	/// Represents a text box control for inputting and displaying currency values.
	/// </summary>
	/// <remarks>The <see cref="ARSCurrencyTextBox"/> provides functionality for handling currency input,  including
	/// validation, formatting, and range enforcement. The control automatically formats  the entered value as currency
	/// based on the specified culture and ensures that the value  remains within the defined range.</remarks>
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

	/// <summary>
	/// Represents a text box control that allows input of integer values within a specified range.
	/// </summary>
	/// <remarks>The <see cref="IntegerTextBox"/> ensures that only valid integer values can be entered. If the
	/// input is invalid, the text box reverts to the previous valid value and displays the text in red. The control also
	/// supports specifying a minimum and maximum allowable value through the <see cref="MinValue"/> and <see
	/// cref="MaxValue"/> properties.</remarks>
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

	/// <summary>
	/// Represents a text box control designed for email address input, with validation and formatting behavior.
	/// </summary>
	/// <remarks>The <see cref="EmailTextBox"/> validates the text entered by the user to ensure it is a valid email
	/// address. If the text is invalid, the control reverts to the previous value and displays the text in red. If the
	/// text is valid, it updates the internal state and displays the text in the default color.</remarks>
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

			if (!DocumentValidations .IsEmail(Text))
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

	/// <summary>
	/// Represents a specialized text box control designed for handling document-specific input fields.
	/// </summary>
	/// <remarks>The <see cref="DocumentTextBox"/> class provides functionality for validating and formatting text
	/// input  according to document-specific requirements, such as applying masks and ensuring valid formats.  It supports
	/// features like masking raw input, restricting allowed key presses, and validating input  based on custom
	/// rules.</remarks>
	public abstract class DocumentTextBox : ARSTextBox, IDocumentField
	{
		public bool ApplyMaskOnFocusLeave { get; set; } = true;

		public override int MaxLength => base.MaxLength;

		protected abstract int UnmaskedLength { get; }
		protected abstract int MaskedLength { get; }
		protected abstract bool IsValidFormat(string text);
		protected abstract string ApplyMask(string rawText);

		protected override void OnMouseEnter(EventArgs e)
		{
			if (base.MaxLength == MaskedLength)
				base.MaxLength = UnmaskedLength;

			base.OnMouseEnter(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			bool isDigitKey =
				(e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
				(e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9);

			bool isControlShortcut =
				e.Control && (
					e.KeyCode == Keys.C ||
					e.KeyCode == Keys.V ||
					e.KeyCode == Keys.X
				);

			bool isAllowedKey =
				isDigitKey ||
				isControlShortcut ||
				e.KeyCode == Keys.Back ||
				e.KeyCode == Keys.Tab ||
				e.KeyCode == Keys.Delete;

			if (!isAllowedKey)
			{
				e.SuppressKeyPress = true;
				return;
			}

			base.OnKeyDown(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			IsValid = ValidateInput();
			base.OnLostFocus(e);
		}

		public override bool ValidateInput()
		{
			if (IsRequired && string.IsNullOrWhiteSpace(Text))
			{
				IsValid = false;
				return false;
			}

			if (!IsValidFormat(Text))
				return false;

			var rawText = Text.Replace(".", "").Replace("-", "").Replace("/", "").Trim();

			if (ApplyMaskOnFocusLeave && rawText.Length == UnmaskedLength)
			{
				Text = ApplyMask(rawText);
				base.MaxLength = MaskedLength;
			}

			return true;
		}
	}

	public class CEPTextBox : DocumentTextBox
	{
		protected override int UnmaskedLength => 8;
		protected override int MaskedLength => 9;

		protected override bool IsValidFormat(string text) => DocumentValidations.IsCep(text);

		protected override string ApplyMask(string rawText) =>
			rawText.Insert(5, "-");
	}


	public class CPFTextBox : DocumentTextBox
	{
		protected override int UnmaskedLength => 11;
		protected override int MaskedLength => 14;

		protected override bool IsValidFormat(string text) => DocumentValidations.IsCPF(text);

		protected override string ApplyMask(string rawText) =>
			rawText.Insert(3, ".").Insert(7, ".").Insert(11, "-");
	}

	public class CNPJTextBox : DocumentTextBox
	{
		protected override int UnmaskedLength => 14;
		protected override int MaskedLength => 18;

		protected override bool IsValidFormat(string text) => DocumentValidations.IsCNPJ(text);

		protected override string ApplyMask(string rawText) =>
			rawText.Insert(2, ".").Insert(6, ".").Insert(10, "/").Insert(15, "-");
	}
	 
	#endregion



}
