using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Shared.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentValidation;
using FluentValidation.Results;
using Plugin.Maui.OCR;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace CarMaintenanceDiary.Mobile.ViewModels
{
    public partial class VehicleAddViewModel : ObservableObject
    {
        private readonly IVehicleService _vehicleService;
        private readonly IValidator<VehicleAddViewModel> _validator;

        [ObservableProperty]
        private string make;

        [ObservableProperty]
        private string model;

        [ObservableProperty]
        private string year;

        [ObservableProperty]
        private string licensePlate;

        [ObservableProperty]
        private string vin;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool canSave;

        [ObservableProperty]
        private ObservableCollection<string> validationErrors = new();

        public VehicleAddViewModel(IVehicleService vehicleService, IValidator<VehicleAddViewModel> validator)
        {
            _vehicleService = vehicleService;
            _validator = validator;
        }

        partial void OnMakeChanged(string value) => Validate();
        partial void OnModelChanged(string value) => Validate();
        partial void OnYearChanged(string value) => Validate();
        partial void OnLicensePlateChanged(string value) => Validate();
        partial void OnVinChanged(string value) => Validate();

        private void Validate()
        {
            ValidationResult result = _validator.Validate(this);

            this.validationErrors.Clear();
            foreach (var error in result.Errors)
            {
                this.validationErrors.Add(error.ErrorMessage);
            }

            CanSave = result.IsValid;
        }

        [RelayCommand]
        private async Task AddVehicleAsync()
        {
            if (IsBusy || !CanSave)
                return;

            IsBusy = true;
            try
            {
                if (await _vehicleService.LicensePlateExistsAsync(LicensePlate))
                {
                    await App.Current.MainPage.DisplayAlert("Duplicate", "License plate already exists.", "OK");
                    return;
                }

                var dto = new VehicleDto
                {
                    Make = Make,
                    Model = Model,
                    Year = Year,
                    LicensePlate = LicensePlate,
                    VIN = Vin
                };

                await _vehicleService.AddAsync(dto);
                await Shell.Current.GoToAsync(".."); // Navigate back
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to add vehicle: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task ScanPlateFromGalleryAsync()
        {
            try
            {
                var pickResult = await MediaPicker.Default.PickPhotoAsync();
                if (pickResult == null)
                    return;

                using var imageAsStream = await pickResult.OpenReadAsync();
                var imageAsBytes = new byte[imageAsStream.Length];
                await imageAsStream.ReadAsync(imageAsBytes);
                var ocrResult = await OcrPlugin.Default.RecognizeTextAsync(imageAsBytes);

                var plate = ocrResult.Lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l)).Trim();
                if (!string.IsNullOrEmpty(plate))
                    LicensePlate = plate;
                else
                    await App.Current.MainPage.DisplayAlert("OCR", "No text recognized.", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("OCR Error", $"Could not scan plate: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task ScanPlateFromCameraAsync()
        {
            try
            {
                var captureResult = await MediaPicker.Default.CapturePhotoAsync();
                if (captureResult == null)
                    return;

                using var imageAsStream = await captureResult.OpenReadAsync();
                var imageAsBytes = new byte[imageAsStream.Length];
                await imageAsStream.ReadAsync(imageAsBytes);
                var ocrResult = await OcrPlugin.Default.RecognizeTextAsync(imageAsBytes);

                var plate = ocrResult.Lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l)).Trim();
                if (!string.IsNullOrEmpty(plate))
                    LicensePlate = plate;
                else
                    await App.Current.MainPage.DisplayAlert("OCR", "No text recognized.", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("OCR Error", $"Could not scan plate: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task ScanVinFromGalleryAsync()
        {
            try
            {
                var captureResult = await MediaPicker.Default.PickPhotoAsync();
                if (captureResult == null)
                    return;

                using var imageAsStream = await captureResult.OpenReadAsync();
                var imageAsBytes = new byte[imageAsStream.Length];
                await imageAsStream.ReadAsync(imageAsBytes);

                var ocrResult = await OcrPlugin.Default.RecognizeTextAsync(imageAsBytes);
                var lines = ocrResult.Lines.Select(l => l.ToUpperInvariant().Trim()).ToList();

                // Try to find VIN near lines containing the keyword "VIN"
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Contains("VIN"))
                    {
                        // Check current line and up to 2 surrounding lines
                        for (int offset = -1; offset <= 1; offset++)
                        {
                            int index = i + offset;
                            if (index >= 0 && index < lines.Count)
                            {
                                var text = lines[index].Replace(" ", ""); // remove spaces between characters
                                var match = Regex.Match(text, @"[A-HJ-NPR-Z0-9]{17}");
                                if (match.Success)
                                {
                                    Vin = match.Value;                                   
                                    return;
                                }
                            }
                        }
                    }
                }

                // Fallback: try regular detection if no "VIN" label found
                foreach (var line in lines)
                {
                    var cleaned = line.Replace(" ", "");
                    var match = Regex.Match(cleaned, @"[A-HJ-NPR-Z0-9]{17}");
                    if (match.Success)
                    {
                        Vin = match.Value;                        
                        return;
                    }
                }

                await App.Current.MainPage.DisplayAlert("OCR", "No VIN number recognized. Please provide picture which focuses as much as close to VIN id", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("OCR Error", $"Could not scan VIN: {ex.Message}", "OK");
            }
        }
    }
}
