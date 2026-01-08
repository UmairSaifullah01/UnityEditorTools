# Unity Editor Tools

A comprehensive Unity package that enhances the Unity Inspector with powerful attributes, property drawers, validators, and utility tools to improve your development workflow.

## 📦 Features

### 🎨 Property Drawers
- **AnimatorParam** - Dropdown for Animator parameters
- **CurveRange** - Constrained animation curve editor
- **Dropdown** - Custom dropdown selection
- **EnumFlags** - Enhanced enum flags display
- **Expandable** - Expandable object references
- **InputAxis** - Dropdown for Unity Input Manager axes
- **Layer** - Dropdown for Unity layers
- **MinMaxSlider** - Range slider for min/max values
- **ProgressBar** - Visual progress bar display
- **ResizableTextArea** - Auto-resizing text area
- **Scene** - Scene asset picker
- **ShowAssetPreview** - Asset preview in inspector
- **SortingLayer** - Dropdown for sorting layers
- **Switch** - Toggle switch UI element
- **Tag** - Dropdown for Unity tags
- **ReorderableList** - Reorderable list for arrays
- **Button** - Method buttons in inspector

### 🎯 Meta Attributes
- **BoxGroup** - Group properties in a box
- **Foldout** - Collapsible property groups
- **Label** - Custom property labels
- **ReadOnly** - Make properties read-only
- **ShowIf** / **HideIf** - Conditional visibility
- **EnableIf** / **DisableIf** - Conditional interactivity
- **OnValueChanged** - Callback when value changes

### ✅ Validators
- **Required** - Mark fields as required
- **MinValue** / **MaxValue** - Numeric range validation
- **ValidateInput** - Custom validation with messages

### 🛠️ Utility Tools

#### Quick Export Asset
An editor window for quickly organizing and exporting Unity assets into structured packages. Features:
- Automatic asset organization by type (Textures, Models, Scripts, Prefabs, etc.)
- Creates organized folder structure
- Dependency checking
- Export to `.unitypackage`
- Revert changes functionality
- Supports nested folder paths

**Access:** `Window > THEBADDEST > Quick Export Asset`

#### Auto Save Project
Automatically saves your Unity project at configurable intervals to prevent data loss.

#### ScriptableObject Creator
Quick access to create ScriptableObject instances from the Project window.

## 🚀 Installation

### Via Package Manager (Git URL)
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button
3. Select `Add package from git URL...`
4. Enter: `https://github.com/UmairSaifullah01/UnityEditorTools.git`

### Via Unity Package
1. Download the latest `.unitypackage` from releases
2. Import into your Unity project (`Assets > Import Package > Custom Package...`)

### Requirements
- Unity 2020.3 or later

## 📖 Usage Examples

### Basic Property Drawers

```csharp
using UnityEngine;
using THEBADDEST;

public class ExampleScript : MonoBehaviour
{
    [Tag]
    public string playerTag;
    
    [Layer]
    public int enemyLayer;
    
    [Scene]
    public string gameScene;
    
    [MinMaxSlider(0, 100)]
    public Vector2 healthRange;
    
    [ProgressBar("Health", 100, EColor.Green)]
    public float health = 75f;
    
    [ResizableTextArea]
    public string description;
    
    [ShowAssetPreview]
    public Sprite icon;
}
```

### Conditional Visibility

```csharp
public class ConditionalExample : MonoBehaviour
{
    public bool showAdvanced;
    
    [ShowIf("showAdvanced")]
    public float advancedValue;
    
    [HideIf("showAdvanced")]
    public float simpleValue;
    
    [EnableIf("showAdvanced")]
    public int configurableValue;
    
    [DisableIf("showAdvanced")]
    public int lockedValue;
}
```

### Validation

```csharp
public class ValidationExample : MonoBehaviour
{
    [Required]
    public GameObject target;
    
    [MinValue(0)]
    public int positiveNumber;
    
    [MaxValue(100)]
    public float maxPercentage;
    
    [ValidateInput("IsValidName", "Name must be at least 3 characters")]
    public string playerName;
    
    private bool IsValidName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length >= 3;
    }
}
```

### Grouping Properties

```csharp
public class GroupedExample : MonoBehaviour
{
    [BoxGroup("Player Settings")]
    public string playerName;
    
    [BoxGroup("Player Settings")]
    public int playerLevel;
    
    [Foldout("Advanced Settings")]
    public bool enableDebug;
    
    [Foldout("Advanced Settings")]
    public float debugValue;
}
```

### Quick Export Asset Workflow

1. Open `Window > THEBADDEST > Quick Export Asset`
2. Enter a package name (e.g., "MyGameAssets")
3. Click **Create Folders** to generate the folder structure
4. Select assets in the Project window or add them to "Additional Objects"
5. Click **Organise** to automatically sort assets into appropriate folders
6. Click **Export Package** to create a `.unitypackage` file
7. Use **Revert Changes** if you need to undo the organization

## 📚 Available Attributes

### Drawer Attributes
| Attribute | Description |
|-----------|-------------|
| `[AnimatorParam]` | Dropdown for Animator Controller parameters |
| `[CurveRange]` | Animation curve with min/max constraints |
| `[Dropdown]` | Custom dropdown from method or values |
| `[EnumFlags]` | Enhanced enum flags with checkboxes |
| `[Expandable]` | Expandable object reference in inspector |
| `[InputAxis]` | Dropdown for Unity Input Manager axes |
| `[Layer]` | Dropdown for Unity layers |
| `[MinMaxSlider]` | Range slider for Vector2 min/max |
| `[ProgressBar]` | Visual progress bar with color coding |
| `[ResizableTextArea]` | Auto-resizing text area field |
| `[Scene]` | Scene asset picker |
| `[ShowAssetPreview]` | Display asset preview thumbnail |
| `[SortingLayer]` | Dropdown for sorting layers |
| `[Switch]` | Toggle switch UI element |
| `[Tag]` | Dropdown for Unity tags |
| `[Button]` | Method button in inspector |

### Meta Attributes
| Attribute | Description |
|-----------|-------------|
| `[BoxGroup]` | Group properties in a visual box |
| `[Foldout]` | Collapsible property group |
| `[Label]` | Custom label text for properties |
| `[ReadOnly]` | Make property read-only in inspector |
| `[ShowIf]` | Show property conditionally |
| `[HideIf]` | Hide property conditionally |
| `[EnableIf]` | Enable property conditionally |
| `[DisableIf]` | Disable property conditionally |
| `[OnValueChanged]` | Callback when property value changes |

### Validator Attributes
| Attribute | Description |
|-----------|-------------|
| `[Required]` | Field must not be null/empty |
| `[MinValue]` | Minimum numeric value |
| `[MaxValue]` | Maximum numeric value |
| `[ValidateInput]` | Custom validation method |

## 🔧 Advanced Features

### Custom Property Drawers
All property drawers extend `PropertyDrawerBase` and can be customized or extended for your specific needs.

### Reflection Utilities
The package includes powerful reflection utilities for accessing fields, properties, and methods at runtime.

### Button Utility
Add buttons to your inspector that call methods:
```csharp
using THEBADDEST;

public class ExampleWithButtons : MonoBehaviour
{
    [Button("Do Something")]
    private void DoSomething()
    {
        Debug.Log("Button clicked!");
    }
    
    [Button("Reset", EButtonEnableMode.Playmode)]
    private void ResetValues()
    {
        // Only enabled in play mode
    }
}
```

## 📝 Notes

- All attributes work with the default Unity Inspector
- The `UInspector` custom editor automatically handles all attributes
- Property drawers support nested objects and arrays
- Validation occurs in the editor and provides immediate feedback

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**Umair Saifullah**
- Website: [umairsaifullah.com](https://www.umairsaifullah.com)
- Email: contact@umairsaifullah.com
- GitHub: [@UmairSaifullah01](https://github.com/UmairSaifullah01)

## 🙏 Acknowledgments

Inspired by the need for better Unity Inspector tools and workflow improvements.

---

Made with ❤️ for the Unity community
