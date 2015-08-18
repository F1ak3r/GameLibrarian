using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Data.Json;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace GameLibrarian.Data
{
    /// <summary>
    /// Base class for <see cref="GameType"/> and <see cref="CategoryType"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class Item : GameLibrarian.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        // Properties

        public Item(String uniqueId, String title, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(Item._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// GameType item data model.
    /// </summary>
    public class GameType : Item
    {
        public GameType(String uniqueId, String title, String imagePath, String description, List<string> executables, List<string> textfiles, CategoryType category, SubcategoryType subcategory)
            : base(uniqueId, title, imagePath, description)
        {
            this._executables = executables;
            this._textfiles = textfiles;
            this._category = category;
            this._subcategory = subcategory;
        }

        private CategoryType _category;
        public CategoryType Category
        {
            get { return this._category; }
            set { this.SetProperty(ref this._category, value); }
        }

        private SubcategoryType _subcategory;
        public SubcategoryType Subcategory
        {
            get { return this._subcategory; }
            set { this.SetProperty(ref this._subcategory, value); }
        }

        private List<string> _executables;
        public List<string> Executables
        {
            get { return this._executables; }
        }

        private List<string> _textfiles;
        public List<string> Textfiles
        {
            get { return this._textfiles; }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class CategoryType : Item
    {
        public CategoryType(String uniqueId, String title, String imagePath, String description, String folder)
            : base(uniqueId, title, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
            this._folder = folder;
            this._subcategories = new List<SubcategoryType>();
        }

        protected void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private string _folder;
        public string Folder
        {
            get { return this._folder; }
        }

        private List<SubcategoryType> _subcategories;
        public List<SubcategoryType> Subcategories
        {
            get { return this._subcategories; }
            set { this.SetProperty(ref this._subcategories, value); }
        }

        private ObservableCollection<GameType> _items = new ObservableCollection<GameType>();
        public ObservableCollection<GameType> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<GameType> _topItem = new ObservableCollection<GameType>();
        public ObservableCollection<GameType> TopItems
        {
            get {return this._topItem; }
        }

        public bool HasSubCategories()
        {
            return (_subcategories.Count == 0);
        }
    }

    public class SubcategoryType : CategoryType
    {
        public SubcategoryType(String uniqueId, String title, String imagePath, String description, String folder)
            : base(uniqueId, title, imagePath, description, folder)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private ObservableCollection<GameType> _items = new ObservableCollection<GameType>();
        public ObservableCollection<GameType> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<GameType> _topItem = new ObservableCollection<GameType>();
        public ObservableCollection<GameType> TopItems
        {
            get { return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// DataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class DataSource
    {
        private static DataSource _DataSource = new DataSource();

        private ObservableCollection<CategoryType> _allGroups = new ObservableCollection<CategoryType>();
        public ObservableCollection<CategoryType> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<CategoryType> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _DataSource.AllGroups;
        }

        public static void AddGroup(CategoryType group)
        {
            _DataSource.AllGroups.Add(group);
        }

        public static CategoryType GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _DataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static GameType GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _DataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async void makeCategories()
        {
            FolderPicker folderpicker = new FolderPicker();
            folderpicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderpicker.PickSingleFolderAsync();

            IReadOnlyList<StorageFolder> folders = await folder.GetFoldersAsync();

            List<string> hasSubcats = new List<string>() {
                            "Compilations",
                            "Interactive Fiction",
                            "Non-Games",
                            "Platform"
                        };

            foreach (StorageFolder f in folders)
            {
                if (f.Name.StartsWith("zzz"))
                    continue;

                var c = new CategoryType(f.Name.Replace(" ", "-"),
                                    f.Name,
                                    "Assets/DarkGray.png",
                                    "A category",
                                    f.Path
                                    );

                if (hasSubcats.Contains(c.Title))
                    this.makeSubcategories(c);
                else
                    this.makeGames(c);

                this.AllGroups.Add(c);
            }

            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
        }

        private async void makeSubcategories(CategoryType category)
        {
            StorageFolder cfolder = await StorageFolder.GetFolderFromPathAsync(category.Folder);
            IReadOnlyList<StorageFolder> folders = await cfolder.GetFoldersAsync();

            foreach (StorageFolder f in folders)
            {
                if (f.Name.StartsWith("zzz"))
                    continue;

                var s = new SubcategoryType(f.Name.Replace(" ", "-"),
                            f.Name,
                            "Assets/DarkGray.png",
                            "A subcategory",
                            f.Path
                            );

                category.Subcategories.Add(s);

                makeGames(category, s);
            }
        }

        private async void makeGame(StorageFolder gf, CategoryType category, SubcategoryType subcategory)
        { 
                IReadOnlyList<StorageFile> gameFiles = await gf.GetFilesAsync();

                List<string> executables, textfiles;

                try {
                    executables = (from f in gameFiles where f.Name.EndsWith(".exe") select f.Path).ToList();
                }
                catch (InvalidOperationException e) {
                    executables = null;
                }

                try {
                    textfiles = (from f in gameFiles where f.Name.EndsWith(".txt") select f.Path).ToList();
                }
                catch (InvalidOperationException e) {
                    textfiles = null;
                }

                var g = new GameType(gf.Name.Replace(" ", "-"),
                            gf.Name,
                            "Assets/LightGray.png",
                            "A game",
                            executables,
                            textfiles,
                            category,
                            subcategory
                            );

                category.Items.Add(g);
        }

        private async void makeGames(CategoryType category, SubcategoryType subcategory=null)
        {   
            IReadOnlyList<StorageFolder> folders;
            if (subcategory != null)
            {
                StorageFolder sfolder = await StorageFolder.GetFolderFromPathAsync(subcategory.Folder);
                folders = await sfolder.GetFoldersAsync();
            }
            else
            {
                StorageFolder cfolder = await StorageFolder.GetFolderFromPathAsync(category.Folder);
                folders = await cfolder.GetFoldersAsync();
            }

            foreach (StorageFolder gf in folders)
            {
                makeGame(gf, category, subcategory);
            }
        }

        private static JsonObject dataToJson()
        {
            JsonObject state = new JsonObject();
            JsonArray categories = new JsonArray();

            foreach (CategoryType c in DataSource.GetGroups("AllGroups"))
            {
                JsonObject category = new JsonObject();
                category["Title"] = JsonValue.CreateStringValue(c.Title);
                category["Description"] = JsonValue.CreateStringValue(c.Description);
                category["Folder"] = JsonValue.CreateStringValue(c.Folder);

                JsonArray subcategories = new JsonArray();
                foreach (SubcategoryType s in c.Subcategories)
                { 
                    JsonObject subcategory = new JsonObject();
                    subcategory["Title"] = JsonValue.CreateStringValue(s.Title);
                    subcategory["Description"] = JsonValue.CreateStringValue(s.Description);
                    subcategory["Folder"] = JsonValue.CreateStringValue(s.Folder);

                    subcategories.Add(subcategory);
                }
                category["Subcategories"] = subcategories;

                JsonArray games = new JsonArray();
                foreach (GameType g in c.Items)
                { 
                    JsonObject game = new JsonObject();
                    game["Title"] = JsonValue.CreateStringValue(g.Title);
                    game["Description"] = JsonValue.CreateStringValue(g.Description);

                    JsonArray executables = new JsonArray();
                    foreach (string e in g.Executables)
                        executables.Add(JsonValue.CreateStringValue(e));
                    game["Executables"] = executables;

                    JsonArray textfiles = new JsonArray();
                    foreach (string t in g.Textfiles)
                        executables.Add(JsonValue.CreateStringValue(t));
                    game["Textfiles"] = textfiles;

                    game["Category"] = JsonValue.CreateStringValue(g.Category.ToString());
                    if (g.Subcategory != null)
                        game["Subcategory"] = JsonValue.CreateStringValue(g.Subcategory.ToString());

                    games.Add(game);
                }
                category["Games"] = games;

                categories.Add(category);
            }

            state["Categories"] = categories;

            return state;
        }

        private void jsonToData(string json)
        {
            JsonObject state = JsonObject.Parse(json);

            foreach (var c in state["Categories"].GetArray())
            {
                JsonObject cObj = c.GetObject();
                CategoryType category = new CategoryType(cObj["Title"].GetString().Replace(" ", "-"),
                                    cObj["Title"].GetString(),
                                    "Assets/DarkGray.png",
                                    cObj["Description"].GetString(),
                                    cObj["Folder"].GetString()
                                    );

                //DataSource.AddGroup(category);
                this.AllGroups.Add(category);

                foreach (var s in cObj["Subcategories"].GetArray())
                {
                    JsonObject sObj = s.GetObject();
                    SubcategoryType subcategory = new SubcategoryType(sObj["Title"].GetString().Replace(" ", "-"),
                                        sObj["Title"].GetString(),
                                        "Assets/DarkGray.png",
                                        sObj["Description"].GetString(),
                                        sObj["Folder"].GetString()
                                        );
                    category.Subcategories.Add(subcategory);
                }

                foreach (var g in cObj["Games"].GetArray())
                {
                    JsonObject gObj = g.GetObject();

                    List<string> executables = new List<string>();
                    foreach (var exe in gObj["Executables"].GetArray())
                    {
                        executables.Add(exe.GetString());
                    }

                    List<string> textfiles = new List<string>();
                    foreach (var txt in gObj["Textfiles"].GetArray())
                    {
                        textfiles.Add(txt.Stringify());
                    }

                    GameType game = new GameType(gObj["Title"].GetString().Replace(" ", "-"),
                                        gObj["Title"].GetString(),
                                        "Assets/DarkGray.png",
                                        gObj["Description"].GetString(),
                                        executables,
                                        textfiles,
                                        category,
                                        category.Subcategories.FirstOrDefault(subcat => gObj["Subcategory"].GetString() == subcat.Title)
                                        );

                    category.Items.Add(game);
                }
            }
        }

        public static async void saveJson()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile jsonFile = await folder.CreateFileAsync("games.json", CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(jsonFile, dataToJson().ToString());
        }

        public async void loadJson()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile jsonFile = await folder.GetFileAsync("games.json");
                string json = await FileIO.ReadTextAsync(jsonFile);
                jsonToData(json);
            }
            catch (Exception e)
            {
                makeCategories();
            }            
        }

        public DataSource()
        {
            loadJson();
        }
    }
}
