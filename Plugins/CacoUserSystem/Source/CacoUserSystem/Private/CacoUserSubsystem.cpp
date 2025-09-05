// CacoUserSubsystem.cpp
#include "CacoUserSubsystem.h"
#include "Kismet/GameplayStatics.h"
#include "CacoUserSettings.h"



void UCacoUserSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
    Super::Initialize(Collection);

    CachedSettings = GetDefault<UCacoUserSettings>();
    if (CachedSettings)
    {
        UE_LOG(LogTemp, Log, TEXT("AutoSave: %s"), CachedSettings->bAutoSaveLocalUsers ? TEXT("true") : TEXT("false"));
        UE_LOG(LogTemp, Log, TEXT("SaveFileName: %s"), *CachedSettings->LocalUserSaveFileName);
        UE_LOG(LogTemp, Log, TEXT("RememberLastUser: %s"), CachedSettings->bRememberLastLoggedInUser ? TEXT("true") : TEXT("false"));
        UE_LOG(LogTemp, Log, TEXT("DefaultGuest: %s (%s)"), CachedSettings->bDefaultGuestUser ? TEXT("true") : TEXT("false"), *CachedSettings->DefaultGuestUserName);
    }
}

void UCacoUserSubsystem::Deinitialize()
{
    //SaveLocalUsers();
    Super::Deinitialize();
}
bool UCacoUserSubsystem::Login(const FString& UserId, const FString& DisplayName, const FString& Provider)
{
    CurrentUser = FCacoUser(UserId, DisplayName, Provider);

    // Agregar a LocalUsers si no existe
    if (!LocalUsers.ContainsByPredicate([&](const FCacoUser& User) { return User.UserId == UserId; }))
    {
        LocalUsers.Add(CurrentUser);
    }

    SaveLocalUsers();
    SaveLastLoggedInUser();
    return true;
}

void UCacoUserSubsystem::Logout()
{
    CurrentUser = FCacoUser();
}

FCacoUser UCacoUserSubsystem::GetCurrentUser() const
{
    return CurrentUser;
}

FCacoUser UCacoUserSubsystem::GetLastLoggedInUser() const
{
    FCacoUser LastUser;
    // Aquí cargarías del archivo guardado
    return LastUser;
}

const TArray<FCacoUser>& UCacoUserSubsystem::GetLocalUsers() const
{
    return LocalUsers;
}

void UCacoUserSubsystem::SaveLocalUsers()
{
    // TODO: Guardar LocalUsers en archivo JSON o SaveGame
}

void UCacoUserSubsystem::LoadLocalUsers()
{
    // TODO: Cargar LocalUsers desde archivo
}

void UCacoUserSubsystem::SaveLastLoggedInUser()
{
    // TODO: Guardar CurrentUser como último usuario logueado
}

void UCacoUserSubsystem::LoadLastLoggedInUser()
{
    // TODO: Cargar último usuario logueado
}
