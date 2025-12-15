using System.Collections.ObjectModel;
using urban_city_power_managment.Services;

namespace urban_city_power_managment.ViewModels
{
    public class TasksViewModel : ViewModelBase
    {
private readonly ILocalizationService _localizationService;

public TasksViewModel(ILocalizationService localizationService)
    {
          _localizationService = localizationService;
    Tasks = new ObservableCollection<TaskItem>
  {
        new TaskItem 
         { 
         Name = "Inspect wind turbines",
   Status = "Pending",
             Priority = "High"
      },
        new TaskItem 
        { 
  Name = "Quarterly energy report",
     Status = "In Progress",
         Priority = "Medium"
}
   };
      }

 public string Title => _localizationService.GetString("Tasks");
      public ObservableCollection<TaskItem> Tasks { get; }
    }

  public class TaskItem
    {
  public string Name { get; set; } = string.Empty;
   public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}
