#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "CacoUserSubsystem.generated.h"

class UCacoUserSettings;

// Estructura para representar un usuario local
USTRUCT(BlueprintType)
struct CACOUSERSYSTEM_API FCacoUser
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly)
    FString UserId;

    UPROPERTY(BlueprintReadOnly)
    FString DisplayName;

    UPROPERTY(BlueprintReadOnly)
    FString Provider; // EOS, Guest, Google, etc.

    FCacoUser() {}
    FCacoUser(const FString& InUserId, const FString& InDisplayName, const FString& InProvider)
        : UserId(InUserId), DisplayName(InDisplayName), Provider(InProvider) {
    }
};

//
// Subsystem para manejar usuarios dentro del juego
//
UCLASS(Blueprintable)
class CACOUSERSYSTEM_API UCacoUserSubsystem : public UGameInstanceSubsystem
{
    GENERATED_BODY()

public:

    virtual void Initialize(FSubsystemCollectionBase& Collection) override;
    virtual void Deinitialize() override;

    // Login de un usuario 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    bool Login(const FString& UserId, const FString& DisplayName, const FString& Provider);

    // Logout del usuario actual 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    void Logout();

    // Obtiene el usuario actualmente logueado 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    FCacoUser GetCurrentUser() const;

    // Obtiene el último usuario logueado 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    FCacoUser GetLastLoggedInUser() const;

    // Obtiene todos los usuarios guardados en este dispositivo 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    const TArray<FCacoUser>& GetLocalUsers() const;

protected:

    // Usuario actualmente logueado
    UPROPERTY()
    FCacoUser CurrentUser;

    // Lista de usuarios guardados en el dispositivo
    UPROPERTY()
    TArray<FCacoUser> LocalUsers;

    // Guarda los usuarios en un archivo local (SaveGame o JSON)
    void SaveLocalUsers();

    // Carga los usuarios guardados en el dispositivo
    void LoadLocalUsers();

    // Guarda el último usuario logueado
    void SaveLastLoggedInUser();

    // Carga el último usuario logueado
    void LoadLastLoggedInUser();

    UPROPERTY()
    const UCacoUserSettings* CachedSettings; 

};