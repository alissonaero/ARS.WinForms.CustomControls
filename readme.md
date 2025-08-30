# ARSoft.LegacyWinForms.CustomControls;

A comprehensive Windows Forms control library that extends standard WinForms controls with enhanced validation, formatting, and user experience features. Perfect for building robust desktop applications with professional input handling and document validation capabilities.

## 🌟 Features

- **Enhanced Text Controls** - Smart text boxes with built-in validation and formatting
- **Document Validation** - Support for Brazilian documents (CPF, CNPJ, CEP)
- **Specialized Input Controls** - Currency, numeric, email, and integer input handling
- **Improved Labels** - Temporary message display with auto-restore functionality
- **Settings Management** - Easy import/export of application settings
- **Validation Framework** - Consistent validation across all controls
- **Culture Support** - Localization-ready with culture-aware formatting

## 📦 Installation

```bash
# Via NuGet Package Manager
Install-Package ARSoft.LegacyWinForms.CustomControls;

# Via .NET CLI
dotnet add package ARSoft.LegacyWinForms.CustomControls;
```

## 🚀 Quick Start

### Basic Text Input with Validation

```csharp
using ARSoft.LegacyWinForms.CustomControls;

// Create a required text box with validation
var nameTextBox = new ARSTextBox
{
    IsRequired = true,
    RequiredFieldLabel = nameLabel // Label turns red if validation fails
};

// Add placeholder text
nameTextBox.SetPlaceholder("Enter your full name");

// Check validation
if (nameTextBox.IsValid)
{
    Console.WriteLine($"Valid input: {nameTextBox.Text}");
}
```

### Document Validation Controls

#### CPF Input
```csharp
var cpfTextBox = new CPFTextBox
{
    IsRequired = true,
    ApplyMaskOnFocusLeave = true // Automatically formats as XXX.XXX.XXX-XX
};

// Get clean CPF value (numbers only)
string cleanCpf = cpfTextBox.TypedValue; // Returns null if invalid
```

#### CNPJ Input
```csharp
var cnpjTextBox = new CNPJTextBox
{
    IsRequired = true,
    ApplyMaskOnFocusLeave = true // Formats as XX.XXX.XXX/XXXX-XX
};

// Validate and get clean value
if (cnpjTextBox.IsValid)
{
    string cleanCnpj = cnpjTextBox.TypedValue;
}
```

#### Brazilian Postal Code (CEP)
```csharp
var cepTextBox = new CEPTextBox
{
    IsRequired = true,
    ApplyMaskOnFocusLeave = true // Formats as XXXXX-XXX
};

string cleanCep = cepTextBox.TypedValue; // Returns numbers only
```

### Numeric Input Controls

#### Currency Input
```csharp
var priceTextBox = new ARSCurrencyTextBox
{
    MinValue = 0,
    MaxValue = 999999.99,
    Culture = new CultureInfo("pt-BR") // Brazilian currency format
};

// Set and get currency values
priceTextBox.TypedValue = 1250.75; // Displays as R$ 1.250,75
double price = priceTextBox.TypedValue;
```

#### Integer Input
```csharp
var quantityTextBox = new IntegerTextBox
{
    MinValue = 1,
    MaxValue = 1000,
    IsRequired = true
};

int quantity = quantityTextBox.TypedValue;
```

#### Double/Decimal Input
```csharp
var weightTextBox = new DoubleTextBox
{
    MinValue = 0.1,
    MaxValue = 999.99
};

double weight = weightTextBox.TypedValue;
```

### Email Validation
```csharp
var emailTextBox = new EmailTextBox
{
    IsRequired = true,
    RequiredFieldLabel = emailLabel
};

// Email format is automatically validated on focus leave
// Invalid emails revert to previous value and show in red
```

### Enhanced Labels with Temporary Messages

#### Standard Label with Auto-Restore
```csharp
var statusLabel = new ARSLabel
{
    InitialText = "Ready",
    RestoreInitialTextAfterTimeout = true,
    TextTimeout = 3000 // 3 seconds
};

// Show temporary message
statusLabel.ShowTemporary("Processing...", Color.Blue);
// Automatically reverts to "Ready" after 3 seconds
```

#### ToolStrip Label
```csharp
var toolStripStatus = new ARSToolStripLabel
{
    InitialText = "Ready",
    RestoreInitialTextAfterTimeout = true,
    TextTimeout = 5000
};

toolStripStatus.ShowTemporary("File saved successfully!");
```

### Password Input with Reveal Feature
```csharp
var passwordTextBox = new ARSTextBox
{
    UseSystemPasswordChar = true,
    ShowPasswordOnMouseOver = true // Reveals password on hover
};
```

### Settings Management

#### Export Settings
```csharp
var settingsManager = new SettingsManager();

try
{
    settingsManager.Export(@"C:\backup\app-settings.config");
    MessageBoxHelper.ShowInfo("Settings exported successfully!");
}
catch (Exception ex)
{
    MessageBoxHelper.ShowError($"Failed to export settings: {ex.Message}");
}
```

#### Import Settings
```csharp
var settingsManager = new SettingsManager();

try
{
    settingsManager.Import(
        @"C:\backup\app-settings.config", 
        "MyApp.Properties.Settings"
    );
    MessageBoxHelper.ShowInfo("Settings imported successfully!");
}
catch (Exception ex)
{
    MessageBoxHelper.ShowError($"Failed to import settings: {ex.Message}");
}
```

## 🛠️ Utility Classes

### Message Box Helpers
```csharp
// Simplified message boxes
MessageBoxHelper.ShowError("Something went wrong!");
MessageBoxHelper.ShowWarning("Please check your input");
MessageBoxHelper.ShowInfo("Operation completed successfully");
```

### String Utilities
```csharp
// Convert to title case
string title = Util.ToTitleCase("hello world"); // "Hello World"

// Capitalize first letter only
string name = Util.FirstLetterToUpper("john"); // "John"
```

### Document Validation
```csharp
// Validate documents without UI controls
bool isValidCpf = DocumentValidations.IsCPF("123.456.789-00");
bool isValidCnpj = DocumentValidations.IsCNPJ("12.345.678/0001-95");
bool isValidCep = DocumentValidations.IsCep("12345-678");
bool isValidEmail = DocumentValidations.IsEmail("user@example.com");
```

## 🎨 Advanced Features

### Form-Wide Validation
```csharp
private bool ValidateForm()
{
    var controls = this.GetAllControls<ICustomControlsARS>();
    return controls.All(control => control.IsValid);
}

private void SaveButton_Click(object sender, EventArgs e)
{
    if (!ValidateForm())
    {
        MessageBoxHelper.ShowWarning("Please fix validation errors before saving.");
        return;
    }
    
    // Proceed with save operation
}
```

### Cultural Formatting
```csharp
var currencyBox = new ARSCurrencyTextBox();

// Brazilian format
currencyBox.Culture = new CultureInfo("pt-BR");
currencyBox.TypedValue = 1000; // Displays: R$ 1.000,00

// US format
currencyBox.Culture = new CultureInfo("en-US");
currencyBox.TypedValue = 1000; // Displays: $1,000.00
```

## 🔧 Interface Overview

| Interface | Purpose |
|-----------|---------|
| `ICustomControlsARS` | Base interface for validation and required field handling |
| `ICustomLabelARS` | Interface for labels with temporary message capabilities |
| `INumericRangeControlARS` | Interface for controls with min/max value constraints |
| `IDocumentField` | Interface for document input controls with masking |

## 📋 Control Summary

| Control | Purpose | Key Features |
|---------|---------|--------------|
| `ARSTextBox` | Enhanced text input | Validation, placeholders, password reveal |
| `CPFTextBox` | Brazilian CPF input | Auto-masking, validation, clean value extraction |
| `CNPJTextBox` | Brazilian CNPJ input | Auto-masking, validation, clean value extraction |
| `CEPTextBox` | Brazilian postal code | Auto-masking, validation |
| `EmailTextBox` | Email address input | Format validation, error highlighting |
| `DoubleTextBox` | Decimal number input | Range validation, culture-aware parsing |
| `IntegerTextBox` | Integer input | Range validation, input restriction |
| `ARSCurrencyTextBox` | Currency input | Culture-aware formatting, range validation |
| `ARSLabel` | Enhanced label | Temporary messages, auto-restore, color changes |
| `ARSToolStripLabel` | Enhanced ToolStrip label | Temporary messages for status bars |

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

If you encounter any issues or have questions, please:

1. Check the documentation and examples above
2. Search existing issues on GitHub
3. Create a new issue with detailed information about your problem

---

 