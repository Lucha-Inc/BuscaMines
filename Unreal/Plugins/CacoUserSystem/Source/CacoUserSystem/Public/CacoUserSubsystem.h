#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "User/CacoUserSave.h"
#include "Login/LoginProviderType.h"
#include "User/CacoUser.h"
#include "CacoUserSubsystem.generated.h"
class UCacoUserSettings;



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
    bool Login(const FString& UserId, const FString& DisplayName, const ELoginProviderType Provider);

    // Logout del usuario actual 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    void Logout();

    // Obtiene el usuario actualmente logueado 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    const FCacoUser& GetCurrentUser() const;

    // Obtiene el último usuario logueado 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    const FCacoUser& GetLastLoggedInUser() const;

    // Obtiene todos los usuarios guardados en este dispositivo 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    const TArray<FCacoUser>& GetLocalUsers() const;

    UFUNCTION(BlueprintCallable, Category = "User Management")
    ELoginProviderType GetLastLoginProvider() const;

protected:

    static const FString CacoUserSlotName;
    
    UPROPERTY()
    UCacoUserSave* SaveGameInstance;

    // Usuario actualmente logueado
    UPROPERTY()
    FCacoUser CurrentUser;

    // Guarda los usuarios en un archivo local (SaveGame o JSON)
    void SaveLocalData();

    // Carga los usuarios guardados en el dispositivo
    void LoadLocalData();

    UPROPERTY()
    const UCacoUserSettings* CachedSettings;

};

