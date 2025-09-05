#pragma once

#include "CoreMinimal.h"
#include "Engine/DeveloperSettings.h"
#include "CacoUserSettings.generated.h"

UCLASS(config = Game, defaultconfig, meta = (DisplayName = "Caco User Settings"))
class CACOUSERSYSTEM_API UCacoUserSettings : public UDeveloperSettings
{
    GENERATED_BODY()

public:

    // Si se debe guardar automáticamente la lista de usuarios locales
    UPROPERTY(EditAnywhere, config, Category = "Persistence")
    bool bAutoSaveLocalUsers = true;

    // Nombre del archivo donde se guardan los usuarios en disco
    UPROPERTY(EditAnywhere, config, Category = "Persistence")
    FString LocalUserSaveFileName = TEXT("LocalCacoUsers.json");

    // Si se debe recuperar automáticamente el último usuario logueado
    UPROPERTY(EditAnywhere, config, Category = "Session")
    bool bRememberLastLoggedInUser = true;

    // Si se debe implementar un usuario Guest por defecto
    UPROPERTY(EditAnywhere, config, Category = "Guest User")
    bool bDefaultGuestUser = true;

    // Nombre del usuario por defecto en modo Guest
    UPROPERTY(EditAnywhere, config, Category = "Guest User")
    FString DefaultGuestUserName = TEXT("Guest");
};

