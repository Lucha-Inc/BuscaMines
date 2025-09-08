// CacoUserSubsystem.cpp
#include "CacoUserSubsystem.h"
#include "Kismet/GameplayStatics.h"
#include "Logging/LogMacros.h"
#include "User/CacoUserSettings.h"
#include "User/CacoUserSave.h"
#include "Login/LoginProviderType.h"

DEFINE_LOG_CATEGORY_STATIC(LogCacoUserSubsystem, Log, All);




const FString UCacoUserSubsystem::CacoUserSlotName = TEXT("CacoUserSlot");


void UCacoUserSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
    Super::Initialize(Collection);

    CachedSettings = GetDefault<UCacoUserSettings>();
    LoadLocalData();
}

void UCacoUserSubsystem::Deinitialize()
{
    SaveLocalData();
    Super::Deinitialize();
}
bool UCacoUserSubsystem::Login(const FString& UserId, const FString& DisplayName, const ELoginProviderType Provider)
{
    check(SaveGameInstance);
    CurrentUser = FCacoUser(UserId, DisplayName, Provider);

    if (!SaveGameInstance->LocalUsers.ContainsByPredicate([&](const FCacoUser& User) { return User.UserId == UserId; }))
    {
        SaveGameInstance->LocalUsers.Add(CurrentUser);
    }

    SaveGameInstance->LastLoggedInUser = CurrentUser;
    SaveGameInstance->LastLoginProvider = Provider;
    UE_LOG(LogCacoUserSubsystem, Log, TEXT("Usuario actual: %s"), *CurrentUser.UserId);

    SaveLocalData();
    return true;
}

void UCacoUserSubsystem::Logout()
{
    UE_LOG(LogCacoUserSubsystem, Log, TEXT("Usuario %s ha cerrado sesion."), *CurrentUser.UserId);
    SaveGameInstance->LastLoggedInUser = FCacoUser();
    CurrentUser = FCacoUser();
    SaveLocalData();
}

const FCacoUser& UCacoUserSubsystem::GetCurrentUser() const
{
    return CurrentUser;
}

const FCacoUser& UCacoUserSubsystem::GetLastLoggedInUser() const
{
    static FCacoUser EmptyUser;
    if (SaveGameInstance) {
        return SaveGameInstance->LastLoggedInUser;
    }
    else {
        return EmptyUser;
    }
}

const TArray<FCacoUser>& UCacoUserSubsystem::GetLocalUsers() const
{
    static TArray<FCacoUser> EmptyArray;
    if (SaveGameInstance) {
        return SaveGameInstance->LocalUsers;
    }
    else {
        return EmptyArray;
    }
}

ELoginProviderType UCacoUserSubsystem::GetLastLoginProvider() const
{
    if (SaveGameInstance) {
        return SaveGameInstance->LastLoginProvider;
    }
    else {
        return ELoginProviderType::Guest;
    }
}

void UCacoUserSubsystem::LoadLocalData() {
    SaveGameInstance = Cast<UCacoUserSave>(
        UGameplayStatics::LoadGameFromSlot(CacoUserSlotName, 0)
    );


    if (!SaveGameInstance) {
        UE_LOG(LogCacoUserSubsystem, Warning, TEXT("No se encontro un SaveGame existente. Se creara uno nuevo."));
        SaveGameInstance = Cast<UCacoUserSave>(UGameplayStatics::CreateSaveGameObject(UCacoUserSave::StaticClass()));
    }
    else {
        UE_LOG(LogCacoUserSubsystem, Log, TEXT("SaveGame cargado correctamente."));
    }

    if (SaveGameInstance) {
        CurrentUser = SaveGameInstance ? SaveGameInstance->LastLoggedInUser : FCacoUser();
    }
}


void UCacoUserSubsystem::SaveLocalData() {
    bool bSuccess = UGameplayStatics::SaveGameToSlot(SaveGameInstance, CacoUserSlotName, 0);
    UE_LOG(LogCacoUserSubsystem, Log, TEXT("SaveGame guardado en slot '%s' %s."), *CacoUserSlotName, bSuccess ? TEXT("correctamente") : TEXT("fallo"));
}








