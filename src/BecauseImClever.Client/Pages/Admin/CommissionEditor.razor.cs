namespace BecauseImClever.Client.Pages.Admin;

using System.Text.Json;
using BecauseImClever.Application.Commissions;
using BecauseImClever.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

/// <summary>Administrator editing with explicit saves and optimistic concurrency.</summary>
public partial class CommissionEditor
{
    private Guid? loadedId;
    private bool initialized;

    [Parameter]
    public Guid? Id { get; set; }

    private CommissionResponse? Product { get; set; }

    private string Title { get; set; } = string.Empty;

    private string Slug { get; set; } = string.Empty;

    private string Description { get; set; } = string.Empty;

    private decimal BasePrice { get; set; }

    private List<OptionForm> Options { get; } = [];

    private Dictionary<Guid, string> ImageAltTexts { get; } = [];

    private IBrowserFile? SelectedFile { get; set; }

    private string UploadAltText { get; set; } = string.Empty;

    private Guid FileInputKey { get; set; } = Guid.NewGuid();

    private bool IsLoading { get; set; }

    private bool IsBusy { get; set; }

    private bool DetailsDirty { get; set; }

    private bool ConfirmDeleteProduct { get; set; }

    private Guid? ConfirmDeleteImage { get; set; }

    private string? ErrorMessage { get; set; }

    private string? SuccessMessage { get; set; }

    private bool HasUnsavedChanges => this.DetailsDirty || this.SelectedFile is not null ||
        !string.IsNullOrEmpty(this.UploadAltText) ||
        this.Product?.Images.Any(i => this.ImageAltTexts.GetValueOrDefault(i.Id) != i.AltText) == true;

    [Inject]
    private ClientCommissionService Service { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        if (this.initialized && this.loadedId == this.Id)
        {
            return;
        }

        this.initialized = true;
        this.loadedId = this.Id;
        this.ErrorMessage = null;
        this.SuccessMessage = null;
        this.Product = null;
        this.IsLoading = false;
        this.ConfirmDeleteProduct = false;
        this.ConfirmDeleteImage = null;
        this.Title = string.Empty;
        this.Slug = string.Empty;
        this.Description = string.Empty;
        this.BasePrice = 0;
        this.Options.Clear();
        this.ImageAltTexts.Clear();
        this.SelectedFile = null;
        this.UploadAltText = string.Empty;
        this.DetailsDirty = false;
        if (this.Id is Guid id)
        {
            this.IsLoading = true;
            try
            {
                var product = await this.Service.GetForEditAsync(id);
                if (this.Id == id)
                {
                    this.SetProduct(product);
                    this.LoadDetails();
                }
            }
            catch (HttpRequestException exception)
            {
                if (this.Id == id)
                {
                    this.ErrorMessage = CommissionDisplay.Error(exception);
                }
            }
            catch (JsonException)
            {
                if (this.Id == id)
                {
                    this.ErrorMessage = "The commission data could not be read. Please try again.";
                }
            }
            finally
            {
                if (this.Id == id)
                {
                    this.IsLoading = false;
                }
            }
        }
    }

    private Task RetryLoadAsync()
    {
        this.initialized = false;
        return this.OnParametersSetAsync();
    }

    private void MarkDirty()
    {
        this.DetailsDirty = true;
        this.SuccessMessage = null;
    }

    private void AddOption()
    {
        this.Options.Add(new());
        this.MarkDirty();
    }

    private void RemoveOption(OptionForm option)
    {
        this.Options.Remove(option);
        this.MarkDirty();
    }

    private Task SaveAsync() => this.ExecuteAsync(async () =>
    {
        var saved = await this.Service.SaveAsync(this.Product, new(
            this.Title,
            this.Slug,
            this.Description,
            this.BasePrice,
            this.Options.Select(o => new CommissionOption(o.Name, o.Price)).ToArray()));
        this.SetProduct(saved);
        this.LoadDetails();
        this.SuccessMessage = "Product saved.";
        if (!this.Id.HasValue)
        {
            this.Navigation.NavigateTo($"admin/commissions/edit/{saved.Id}", replace: true);
        }
    });

    private Task TogglePublicationAsync() => this.ExecuteAsync(async () =>
    {
        this.SetProduct(await this.Service.PublishAsync(this.Product!, !this.Product!.IsPublished));
        this.SuccessMessage = this.Product!.IsPublished ? "Product published." : "Product unpublished.";
    });

    private Task DeleteProductAsync() => this.ExecuteAsync(async () =>
    {
        await this.Service.DeleteAsync(this.Product!);
        this.DetailsDirty = false;
        this.SelectedFile = null;
        this.UploadAltText = string.Empty;
        this.Product = null;
        this.Navigation.NavigateTo("admin/commissions");
    });

    private void SelectImage(InputFileChangeEventArgs args)
    {
        this.ErrorMessage = null;
        this.SelectedFile = null;
        if (args.File.Size > CommissionDefaults.MaxImageBytes)
        {
            this.ErrorMessage = "The image is too large. Choose an image no larger than 5 MiB.";
            return;
        }

        this.SelectedFile = args.File;
    }

    private Task UploadImageAsync() => this.ExecuteAsync(async () =>
    {
        var file = this.SelectedFile!;
        await using var stream = file.OpenReadStream(CommissionDefaults.MaxImageBytes);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        this.SetProduct(await this.Service.UploadImageAsync(this.Product!, new(file.Name, file.ContentType, buffer.ToArray(), this.UploadAltText)));
        this.SelectedFile = null;
        this.UploadAltText = string.Empty;
        this.FileInputKey = Guid.NewGuid();
        this.SuccessMessage = "Image uploaded.";
    });

    private Task SaveImageAltAsync(Guid imageId) => this.ExecuteAsync(async () =>
    {
        this.SetProduct(await this.Service.UpdateImageAsync(this.Product!, imageId, this.ImageAltTexts[imageId]));
        this.SuccessMessage = "Image description saved.";
    });

    private Task MoveImageAsync(int index, int direction) => this.ExecuteAsync(async () =>
    {
        var ids = this.Product!.Images.Select(i => i.Id).ToArray();
        (ids[index], ids[index + direction]) = (ids[index + direction], ids[index]);
        this.SetProduct(await this.Service.ReorderImagesAsync(this.Product, ids));
        this.SuccessMessage = "Image order saved.";
    });

    private Task DeleteImageAsync(Guid imageId) => this.ExecuteAsync(async () =>
    {
        this.SetProduct(await this.Service.DeleteImageAsync(this.Product!, imageId));
        this.ConfirmDeleteImage = null;
        this.SuccessMessage = "Image deleted.";
    });

    private void SetProduct(CommissionResponse product)
    {
        this.Product = product;
        foreach (var image in product.Images)
        {
            this.ImageAltTexts.TryAdd(image.Id, image.AltText);
        }
    }

    private void LoadDetails()
    {
        this.Title = this.Product!.Title;
        this.Slug = this.Product.Slug;
        this.Description = this.Product.Description;
        this.BasePrice = this.Product.BasePrice;
        this.Options.Clear();
        this.Options.AddRange(this.Product.Options.Select(o => new OptionForm { Name = o.Name, Price = o.Price }));
        this.DetailsDirty = false;
    }

    private async Task BeforeNavigationAsync(LocationChangingContext context)
    {
        if (this.HasUnsavedChanges && !await this.JS.InvokeAsync<bool>("confirm", "Leave this page and discard unsaved commission changes?"))
        {
            context.PreventNavigation();
        }
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        if (this.IsBusy)
        {
            return;
        }

        this.IsBusy = true;
        this.ErrorMessage = null;
        this.SuccessMessage = null;
        try
        {
            await action();
        }
        catch (HttpRequestException exception)
        {
            this.ErrorMessage = CommissionDisplay.Error(exception);
        }
        catch (JsonException)
        {
            this.ErrorMessage = "The server response could not be read. Reload to check the saved state; copy any unsaved edits first.";
        }
        catch (IOException)
        {
            this.ErrorMessage = "The image could not be read. Please select it again and retry.";
        }
        finally
        {
            this.IsBusy = false;
        }
    }

    private sealed class OptionForm
    {
        public string Name { get; set; } = string.Empty;

        public decimal? Price { get; set; }
    }
}
