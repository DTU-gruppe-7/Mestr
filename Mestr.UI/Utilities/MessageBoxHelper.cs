using System.Windows;

namespace Mestr.UI.Utilities
{
    public static class MessageBoxHelper
    {
        // Success Messages
        public static void ShowSuccess(string message, string? title = null)
        {
            MessageBox.Show(
                message,
                title ?? "Handling gennemført",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Error Messages
        public static void ShowError(string message, string? title = null)
        {
            MessageBox.Show(
                message,
                title ?? "Fejl",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        // Warning Messages
        public static void ShowWarning(string message, string? title = null)
        {
            MessageBox.Show(
                message,
                title ?? "Advarsel",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        // Confirmation Dialogs
        public static bool ShowConfirmation(string message, string? title = null)
        {
            var result = MessageBox.Show(
                message,
                title ?? "Bekræft handling",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        // Specific standardized messages
        public static class Standard
        {
            // Create operations
            public static void ClientCreated() => 
                ShowSuccess("Klienten blev oprettet.");

            public static void ProjectCreated() => 
                ShowSuccess("Projektet blev oprettet.");

            public static void TransactionCreated() => 
                ShowSuccess("Transaktionen blev oprettet.");

            // Update operations
            public static void ClientUpdated() => 
                ShowSuccess("Klienten blev opdateret.");

            public static void ProjectUpdated() => 
                ShowSuccess("Projektet blev opdateret.");

            public static void TransactionUpdated() => 
                ShowSuccess("Transaktionen blev opdateret.");

            public static void CompanyInfoSaved() => 
                ShowSuccess("Virksomhedsoplysningerne blev gemt.");

            // Delete confirmations
            public static bool ConfirmDeleteClient(string clientName) =>
                ShowConfirmation(
                    $"Er du sikker på, at du vil slette klienten '{clientName}'?\n\nDenne handling kan ikke fortrydes.",
                    "Bekræft sletning");

            public static bool ConfirmDeleteProject(string projectName) =>
                ShowConfirmation(
                    $"Er du sikker på, at du vil slette projektet '{projectName}'?\n\nDenne handling kan ikke fortrydes.",
                    "Bekræft sletning");

            public static bool ConfirmDeleteTransaction(string transactionType) =>
                ShowConfirmation(
                    $"Er du sikker på, at du vil slette denne {transactionType}?\n\nDenne handling kan ikke fortrydes.",
                    "Bekræft sletning");

            // Unsaved changes
            public static bool ConfirmUnsavedChanges() =>
                ShowConfirmation(
                    "Du har ugemte ændringer. Vil du forlade siden uden at gemme?",
                    "Ugemte ændringer");

            // Error messages
            public static void ClientNotFound() =>
                ShowError("Klienten blev ikke fundet.");

            public static void ProjectNotFound() =>
                ShowError("Projektet blev ikke fundet.");

            public static void SaveError(string details) =>
                ShowError($"Handlingen kunne ikke gennemføres.\n\nFejl: {details}");

            public static void DeleteError(string details) =>
                ShowError($"Sletningen kunne ikke gennemføres.\n\nFejl: {details}");

            public static void LoadError(string details) =>
                ShowError($"Data kunne ikke indlæses.\n\nFejl: {details}");

            public static void InvalidCategory() =>
                ShowWarning("Ugyldig kategori valgt.");

            public static void PdfGenerationError(string details) =>
                ShowError($"Faktura kunne ikke genereres.\n\nFejl: {details}");

            // Specific operation errors
            public static void AddTransactionError(string transactionDescription, string details) =>
                ShowWarning($"Kunne ikke tilføje '{transactionDescription}'.\n\nFejl: {details}");

            public static void UpdateTransactionError(string transactionDescription, string details) =>
                ShowWarning($"Kunne ikke opdatere '{transactionDescription}'.\n\nFejl: {details}");
        }
    }
}
