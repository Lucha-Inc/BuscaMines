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

    UFUNCTION(BlueprintCallable, Category = "User Management")
    bool Prueba1(int32 val);

    UFUNCTION(BlueprintCallable, Category = "User Management")
    bool Prueba2();

    // Obtiene el último usuario logueado 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    int32 GetLastLoggedInUserIndex() const;

    // Obtiene todos los usuarios guardados en este dispositivo 
    UFUNCTION(BlueprintCallable, Category = "User Management")
    TMap<int32, FCacoUserStruct>& GetLocalUsers() const;

protected:

    UPROPERTY()
    const UCacoUserSettings* CachedSettings;

};

