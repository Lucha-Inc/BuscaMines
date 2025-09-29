#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "User/CacoUserStruct.h"
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


    UFUNCTION(BlueprintCallable, Category = "CacoUser Management")
    int32 LogIn(int32 UserIndex = -1);

    UFUNCTION(BlueprintCallable, Category = "CacoUser Management")
    int32 CreateNewUser(FString DisplayName);

    UFUNCTION(BlueprintCallable, Category = "CacoUser Management")
    bool Prueba();

protected:

    // Obtiene el último usuario logueado 
    UFUNCTION(BlueprintCallable, Category = "CacoUser Management")
    int32 GetLastLoggedInUserIndex() const;

    // Obtiene todos los usuarios guardados en este dispositivo 
    UFUNCTION(BlueprintCallable, Category = "CacoUser Management")
    TMap<int32, FCacoUserStruct>& GetLocalUsers() const;

    UPROPERTY()
    const UCacoUserSettings* CachedSettings;

};

