# C# Coding Standards and Naming Conventions


| Object Name               | Notation   | Plural |
|:--------------------------|:-----------|:-------|
| Namespace name            | PascalCase | Yes    |
| Class name                | PascalCase | No     |
| Constructor name          | PascalCase | No     |
| Method name               | PascalCase | Yes    |
| Method arguments          | camelCase  | Yes    |
| Local variables           | camelCase  | Yes    |
| Constants name            | PascalCase | No     |
| Field name                | camelCase  | Yes    |
| Properties name           | PascalCase | Yes    |
| Delegate name             | PascalCase | No     |
| Enum type name            | PascalCase | Yes    |

#### 1. Do use PascalCasing for class names and method names:

```csharp
public class ClientActivity
{
  public void ClearStatistics()
  {
    //...
  }
  public void CalculateStatistics()
  {
    //...
  }
}
```

***Why: consistent with the Microsoft's .NET Framework and easy to read.***

#### 2. Do use camelCasing for method arguments and local variables:

```csharp
public class UserLog
{
  public void Add(LogEvent logEvent)
  {
    int itemCount = logEvent.Items.Count;
    // ...
  }
}
```

***Why: consistent with the Microsoft's .NET Framework and easy to read.***

#### 3. Do use noun or noun phrases to name a class. 

```csharp 
public class Employee
{
}
public class BusinessLocation
{
}
public class DocumentCollection
{
}
```

#### 4. Do prefix interfaces with the letter I. Interface names are noun (phrases) or adjectives.

```csharp     
public interface IShape
{
}
public interface IShapeCollection
{
}
public interface IGroupable
{
}
```

#### 5. Do vertically align curly brackets: 

```csharp 
// Correct
class Program
{
  static void Main(string[] args)
  {
    //...
  }
}
```

***Why: Microsoft has a different standard, but developers have overwhelmingly preferred vertically aligned brackets.***

#### 6. Do declare all member variables at the top of a class, with static variables at the very top.

```csharp 
// Correct
public class Account
{
  public static string BankName;
  public static decimal Reserves;      
  public string Number { get; set; }
  public DateTime DateOpened { get; set; }
  public DateTime DateClosed { get; set; }
  public decimal Balance { get; set; }  

  // Constructor
  public Account()
  {
    // ...
  }
}
```

***Why: generally accepted practice that prevents the need to hunt for variable declarations.***

#### 7. Do not use Hungarian notation or any other type identification in identifiers

```csharp
// Correct
int counter;
string name;    
// Avoid
int iCounter;
string strName;
```

***Why: consistent with the Microsoft's .NET Framework and Visual Studio IDE makes determining types very easy (via tooltips).***

#### 8. Do not use Screaming Caps for constants or readonly variables:

```csharp
// Correct
public const string ShippingType = "DropShip";
// Avoid
public const string SHIPPINGTYPE = "DropShip";
```

***Why: consistent with the Microsoft's .NET Framework. Caps grab too much attention.***

#### 9. Use meaningful names for variables. The following example uses seattleCustomers for customers who are located in Seattle:

```csharp
var seattleCustomers = from customer in customers
  where customer.City == "Seattle" 
  select customer.Name;
```

***Why: consistent with the Microsoft's .NET Framework and easy to read.***

#### 10. Avoid using Abbreviations. Exceptions: abbreviations commonly used as names, such as Id, Xml, Ftp, Uri.

```csharp    
// Correct
UserGroup userGroup;
Assignment employeeAssignment;     
// Avoid
UserGroup usrGrp;
Assignment empAssignment; 
// Exceptions
CustomerId customerId;
XmlDocument xmlDocument;
FtpHelper ftpHelper;
UriPart uriPart;
```

***Why: consistent with the Microsoft's .NET Framework and prevents inconsistent abbreviations.***


#### 11. Do use PascalCasing or camelCasing (Depending on the identifier type) for abbreviations 3 characters or more (2 chars are both uppercase when PascalCasing is appropriate or inside the identifier).:

```csharp  
HtmlHelper htmlHelper;
FtpTransfer ftpTransfer, fastFtpTransfer;
UIControl uiControl, nextUIControl;
```

***Why: consistent with the Microsoft's .NET Framework. Caps would grab visually too much attention.***

#### 12. Do not use Underscores in identifiers. Exception: you can prefix private fields with an underscore:

```csharp 
// Correct
public DateTime clientAppointment;
public TimeSpan timeLeft;    
// Avoid
public DateTime client_Appointment;
public TimeSpan time_Left; 
// Exception (Class field)
private DateTime _registrationDate;
```

***Why: consistent with the Microsoft's .NET Framework and makes code more natural to read.***

#### 13. Do use implicit type var for local variable declarations. Exception: primitive types (int, string, double, etc) use predefined names. 

```csharp 
var stream = File.Create(path);
var customers = new Dictionary();
// Exceptions
int index = 100;
string timeSheet;
bool isCompleted;
```

***Why: removes clutter, particularly with complex generic types. Type is easily detected with Visual Studio tooltips.***

#### 14. Do name event handlers (delegates used as types of events) with the "EventHandler" suffix, as shown in the following example:

```csharp 
public delegate void ReadBarcodeEventHandler(object sender, ReadBarcodeEventArgs e);
```

#### 15. DO use two parameters named sender and e in event handlers. The sender parameter represents the object that raised the event. The sender parameter is typically of type object, even if it is possible to employ a more specific type.

```csharp
public void ReadBarcodeEventHandler(object sender, ReadBarcodeEventArgs e)
{
  //...
}
```