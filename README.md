[![Stories in Ready](https://badge.waffle.io/envicase/ReactiveMvvm.png?label=ready&title=Ready)](https://waffle.io/envicase/ReactiveMvvm)
# Reactive-MVVM 아키텍처

envicase([www.envicase.com](https://www.envicase.com)) 개발팀은 서비스의 iOS 클라이언트 응용프로그램을 개발하면서 여러 뷰에서 보여지는 동일한 컨텐트의 상태를 동기화하기 위한 단순하고 효율적인 방법을 고민했고 그 결과로 Rx(Reactive Extensions)와 MVVM(Model View ViewModel) 디자인 패턴을 결합한 새로운 아키텍처를 고안했습니다. envicase 개발팀은 이 아키텍처를 Reactive-MVVM 아키텍처라고 부릅니다. 이 프로젝트는 Reactive-MVVM 아키텍처를 지원하는 프레임워크를 제공합니다.

## 반응형 모델 스트림

응용프로그램에서 모델과 뷰는 일대다 관계를 가집니다. MVVM 디자인 패턴에서 뷰는 뷰모델로 추상화됩니다. 따라서 하나의 모델은 다수의 뷰모델과 관계를 맺을 수 있습니다. 예를 들어 master-detail 인터페이스를 가진 응용프로그램의 경우 마스터 목록의 선택된 항목 뷰와 상세 뷰는 동일한 모델을 표현합니다. 하지만 동일한 모델을 표현한다고 해서 두 뷰가 동일한 기능을 제공하는 것은 아닙니다. 상세 뷰는 항목 뷰가 보여주지 않는 속성을 추가적으로 보여주기도 하고 항목 속성에 대한 편집 기능을 제공할 수도 있습니다.

```text
+------------+--------------------+
| MASTER     | DETAIL             |
+------------+--------------------+
| User 1     | Id   : 2           |
| User 2   < | Name : Hello World |
| User 3     | Email: foo@bar.com |
| User 4     +-----------+--------|
|            |  Restore  |  Save  |
+------------+-----------+--------+
```

이 때 상세 뷰를 통해 모델의 속성을 수정하면 동일한 모델을 표현하는 다른 뷰들은 어떻게 갱신해야하는지 고민해야합니다. Reactive-MVVM은 반응형 모델 스트림을 제공해 이 문제 해결을 도와줍니다. 모델을 직접 다루는 뷰모델은 스트림을 통해 모델의 변화를 감지하고 또 스트림을 통해 모델의 변화를 알립니다.

### 모델 스트림 구독

Reactive-MVVM은 식별자(`Id` 속성)를 통해 모델을 식별하며 모델 구현을 위한 `IModel<TId>` 인터페이스와 `Model<TId>` 추상 클래스를 제공합니다. 뷰모델은 `ReactiveViewModel<TModel, TId>` 클래스를 상속받는 것 만으로 모델 스트림을 구독하게 됩니다.

```csharp
public class User : Model<int>
{
    public User(int id, string name, string email)
        : base(id)
    {
        Name = name;
        Email = email;
    }

    public string Name { get; }
    public string Email { get; }
}

public class UserViewModel : ReactiveViewModel<User, int>
{
    public UserViewModel(User user)
        : base(user)
    {
    }
}
```

`UserViewModel`에 대한 다음과 같은 뷰(XAML) 코드가 있을 때 뷰는 응용프로그램의 모든 곳에서 발생하는 모델 변화를 반영하게 됩니다.

```xaml
<TextBlock Text="{Binding Model.Name, Mode=OneWay}" />
<TextBlock Text="{Binding Model.Email, Mode=OneWay}" />
```

### 새로운 모델 개정 발행

`ReactiveViewModel<TModel, TId>` 클래스를 상속받은 클래스는 `StreamConnection` 속성을 통해 모델의 변경을 발행할 수 있습니다.

```csharp
StreamConnection.Emit(new User(Id, NewName, NewEmail));
```

이제 동일한 모델을 표현하는 응용프로그램의 모든 뷰는 모델의 새로운 상태를 반영합니다. 모델의 상태를 변경하는 뷰모델과 상태 변경을 구독하는 뷰모델은 서로 어떠한 연관도 가질 필요가 없기 때문에 약한 결합도를 가지거나 결합도를 전혀 가지지 않습니다.
