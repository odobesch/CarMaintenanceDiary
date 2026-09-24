using BlazorBootstrap;
using CarMaintenanceDiary.Application.Interfaces;
using CarMaintenanceDiary.Client.Common;
using CarMaintenanceDiary.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CarMaintenanceDiary.Web.Components.Pages.Maintenance;

public partial class Maintenance : ComponentBase, IAsyncDisposable
{
    [Inject] private IVehicleService VehicleService { get; set; } = default!;
    [Inject] private IMaintenanceService MaintenanceService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;
    [Inject] private ILogger<Maintenance> Logger { get; set; } = default!;
    [Inject] protected PreloadService PreloadService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // UI state
    protected bool IsLoading { get; set; } = true;
    protected bool IsSaving
    {
        get; set;
    }
    protected bool IsEditMode
    {
        get; set;
    }

    // Data
    protected List<VehicleDto> Vehicles { get; set; } = new();
    protected int? SelectedVehicleId
    {
        get; set;
    }
    protected VehicleDto? SelectedVehicle => SelectedVehicleId.HasValue ? Vehicles.FirstOrDefault(v => v.Id == SelectedVehicleId.Value) : null;
    protected List<MaintenanceRecordDto> MaintenanceRecords { get; set; } = new();

    // Editing
    protected MaintenanceRecordDto EditingModel { get; set; } = new();
    private MaintenanceRecordDto? _editingRecordRef;

    // Documents
    protected List<MaintenanceDocumentDto> DocumentsForRecord { get; set; } = new();
    private int _documentRecordId;
    protected int? SelectedDocumentId
    {
        get; set;
    }
    protected string? SelectedDocumentUrl
    {
        get; set;
    }

    // Refs
    protected BlazorBootstrap.Modal? maintenanceModal;
    protected BlazorBootstrap.Modal? documentsModal;
    protected BlazorBootstrap.Modal? documentDetailModal;
    protected ConfirmDialog confirmDialog = default!;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        PreloadService.Show(SpinnerColor.Light, "Loading vehicles...");
        try
        {
            Vehicles = await VehicleService.GetAllAsync();
            if (Vehicles.Count > 0)
            {
                SelectedVehicleId = Vehicles[0].Id;
                // No overlay inside; we already show it here
                await LoadMaintenanceRecords(SelectedVehicleId.Value);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load vehicles.");
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Unable to load vehicles."));
        }
        finally
        {
            PreloadService.Hide();
            IsLoading = false;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // If someone changed SelectedVehicleId via binding, reload
        if (SelectedVehicleId.HasValue && (SelectedVehicle?.MaintenanceRecords == null))
        {
            await LoadMaintenanceRecords(SelectedVehicleId.Value);
        }
    }

    private async Task LoadMaintenanceRecords(int vehicleId, bool showOverlay = false)
    {
        if (showOverlay)
            PreloadService.Show(SpinnerColor.Light, "Loading maintenance records...");
        try
        {
            MaintenanceRecords = (await MaintenanceService.GetMaintenanceRecordsAsync(vehicleId))
                .OrderByDescending(m => m.Date)
                .ToList();

            var v = Vehicles.FirstOrDefault(x => x.Id == vehicleId);
            if (v is not null)
                v.MaintenanceRecords = MaintenanceRecords;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load maintenance records.");
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Error loading maintenance data."));
        }
        finally
        {
            if (showOverlay)
                PreloadService.Hide();
        }
    }

    // Vehicle selection callback from VehicleSelector
    private async Task OnVehicleChanged(int? vehicleId)
    {
        SelectedVehicleId = vehicleId;
        MaintenanceRecords.Clear();
        if (vehicleId.HasValue)
            await LoadMaintenanceRecords(vehicleId.Value, showOverlay: true);
    }

    protected void OpenAddMaintenanceModal()
    {
        IsEditMode = false;
        EditingModel = new MaintenanceRecordDto
        {
            Date = DateTime.Today,
            MaintenanceType = string.Empty,
            Description = string.Empty,
            Workshop = string.Empty,
            Cost = 0
        };
        _editingRecordRef = null;
        maintenanceModal?.ShowAsync();
    }

    protected void EditMaintenanceRecord(MaintenanceRecordDto record)
    {
        IsEditMode = true;
        _editingRecordRef = record;
        EditingModel = new MaintenanceRecordDto
        {
            Id = record.Id,
            VehicleId = record.VehicleId,
            Date = record.Date,
            MaintenanceType = record.MaintenanceType,
            Description = record.Description,
            Workshop = record.Workshop,
            Cost = record.Cost,
            PhotoPaths = new List<string>(record.PhotoPaths ?? new())
        };
        maintenanceModal?.ShowAsync();
    }

    protected async Task SaveMaintenanceRecord()
    {
        if (!SelectedVehicleId.HasValue)
        {
            ToastService.Notify(new ToastMessage(ToastType.Warning, "No vehicle selected."));
            return;
        }

        IsSaving = true;
        PreloadService.Show(SpinnerColor.Light, IsEditMode ? "Saving changes..." : "Adding maintenance...");
        try
        {
            if (IsEditMode && _editingRecordRef is not null)
            {
                await MaintenanceService.UpdateMaintenanceRecordAsync(EditingModel);
                ToastService.Notify(new ToastMessage(ToastType.Success, "Maintenance record updated."));
            }
            else
            {
                await MaintenanceService.AddMaintenanceRecordAsync(SelectedVehicleId.Value, EditingModel);
                ToastService.Notify(new ToastMessage(ToastType.Success, "Maintenance record added."));
            }

            await LoadMaintenanceRecords(SelectedVehicleId.Value, showOverlay: false);
            maintenanceModal?.HideAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to save maintenance record.");
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Failed to save record."));
        }
        finally
        {
            PreloadService.Hide();
            IsSaving = false;
        }
    }

    protected void CloseMaintenanceModal() => maintenanceModal?.HideAsync();

    protected Task OnMaintenanceModalHidden()  // modal lifecycle event -> clear ephemeral state
    {
        _editingRecordRef = null;
        EditingModel = new MaintenanceRecordDto();
        return Task.CompletedTask;
    }

    protected async Task DeleteMaintenanceRecord(MaintenanceRecordDto record)
    {
        if (!SelectedVehicleId.HasValue)
            return;

        var confirmed = await confirmDialog.ShowAsync(
            "Delete maintenance record",
            $"Are you sure you want to delete the maintenance entry on {record.Date:d}?"
        );
        if (!confirmed)
        {
            ToastService.Notify(new ToastMessage(ToastType.Secondary, "Delete cancelled."));
            return;
        }

        PreloadService.Show(SpinnerColor.Light, "Deleting...");
        try
        {
            await MaintenanceService.DeleteMaintenanceRecordAsync(record.Id);
            await LoadMaintenanceRecords(SelectedVehicleId.Value, showOverlay: false);
            ToastService.Notify(new ToastMessage(ToastType.Success, "Maintenance record deleted."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete maintenance record.");
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Failed to delete record."));
        }
        finally
        {
            PreloadService.Hide();
        }
    }

    // Documents
    protected async Task OpenDocuments(MaintenanceRecordDto record)
    {
        _documentRecordId = record.Id;

        PreloadService.Show(SpinnerColor.Light, "Loading documents...");
        try
        {           
            DocumentsForRecord = await MaintenanceService.GetDocumentsAsync(_documentRecordId);
            await documentsModal!.ShowAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get document metadata.");
            DocumentsForRecord = new();
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Failed to load documents."));
        }
        finally
        {
            PreloadService.Hide();
        }
    }

    protected Task OnDocumentsModalHidden()
    {
        DocumentsForRecord.Clear();
        return Task.CompletedTask;
    }

    protected async Task CloseDocumentsModal() => await documentsModal!.HideAsync();

    protected string DocumentPreviewUrl(int id, int w = 300, int h = 180)
        => MaintenanceService.GetDocumentUrl(id, w, h);

    protected async Task ShowDocumentDetail(int docId)
    {
        SelectedDocumentId = docId;
        SelectedDocumentUrl = MaintenanceService.GetDocumentUrl(docId);
        await documentDetailModal!.ShowAsync();
    }

    protected async Task CloseDocumentDetailModal()
    {
        SelectedDocumentId = null;
        SelectedDocumentUrl = null;
        await documentDetailModal!.HideAsync();
    }

    protected Task OnDocumentDetailHidden()
    {
        SelectedDocumentId = null;
        SelectedDocumentUrl = null;
        return Task.CompletedTask;
    }

    protected async Task OpenDocInNewTab(int docId)
    {
        var url = MaintenanceService.GetDocumentUrl(docId);
        await JS.OpenInNewTab(url);
    }

    protected async Task DownloadDocument(int docId)
    {
        var url = MaintenanceService.GetDocumentDownloadUrl(docId);
        await JS.OpenInNewTab(url);
    }

    protected async Task DownloadSelectedDocument()
    {
        if (SelectedDocumentId is null)
            return;
        await DownloadDocument(SelectedDocumentId.Value);
    }

    private const long MaxUploadBytes = 20_000_000;
    protected async Task UploadDocumentFiles(InputFileChangeEventArgs e)
    {
        if (_documentRecordId == 0)
        {
            ToastService.Notify(new ToastMessage(ToastType.Warning, "No maintenance record selected for upload."));
            return;
        }

        var files = e.GetMultipleFiles();
        PreloadService.Show(SpinnerColor.Light, $"Uploading {files.Count} file(s)...");
        try
        {
            foreach (var file in files)
            {
                await using var stream = file.OpenReadStream(MaxUploadBytes);
                await MaintenanceService.UploadDocumentAsync(_documentRecordId, stream, file.Name, file.ContentType);
            }

            DocumentsForRecord = await MaintenanceService.GetDocumentsAsync(_documentRecordId);

            if (SelectedVehicleId.HasValue)
                await LoadMaintenanceRecords(SelectedVehicleId.Value, showOverlay: false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Document upload failed.");
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Document upload failed."));
        }
        finally
        {
            PreloadService.Hide();
        }
    }

    protected async Task DeleteDocument(int docId)
    {
        PreloadService.Show(SpinnerColor.Light, "Deleting document...");
        try
        {
            await MaintenanceService.DeleteDocumentAsync(docId);

            // Update local state
            DocumentsForRecord.RemoveAll(d => d.Id == docId);

            if (SelectedDocumentId == docId)
                await CloseDocumentDetailModal();

            if (_documentRecordId != 0)
                DocumentsForRecord = await MaintenanceService.GetDocumentsAsync(_documentRecordId);

            if (SelectedVehicleId.HasValue)
                await LoadMaintenanceRecords(SelectedVehicleId.Value, showOverlay: false);

            ToastService.Notify(new ToastMessage(ToastType.Success, "Document deleted successfully."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete document.");
            ToastService.Notify(new ToastMessage(ToastType.Danger, "Failed to delete document."));
        }
        finally
        {
            PreloadService.Hide();
        }
    }

    protected string GetIconClassForContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return "bi bi-file-earmark"; // generic


        var ct = contentType.ToLowerInvariant();


        if (ct == "application/pdf")
            return "bi bi-file-earmark-pdf-fill";


        if (ct.StartsWith("image/"))
            return "bi bi-image";


        if (ct.Contains("word") || ct.Contains("msword") || ct.Contains("officedocument.wordprocessingml"))
            return "bi bi-file-earmark-word-fill";


        if (ct.Contains("excel") || ct.Contains("officedocument.spreadsheetml"))
            return "bi bi-file-earmark-excel-fill";


        if (ct.Contains("powerpoint") || ct.Contains("officedocument.presentationml"))
            return "bi bi-file-earmark-ppt"; // keep it simple; fill variant optional


        if (ct.Contains("zip") || ct == "application/x-zip-compressed" || ct == "application/zip")
            return "bi bi-file-earmark-zip-fill";


        if (ct.Contains("json") || ct.Contains("xml") || ct.Contains("javascript") || ct.Contains("text"))
            return "bi bi-file-earmark-text";


        return "bi bi-file-earmark";
    }

    public async ValueTask DisposeAsync()
    {
        // No custom JS registration anymore; nothing special to dispose
        await Task.CompletedTask;
    }
}