#pragma once

#include "CoreMinimal.h"
#include "UObject/Interface.h"
#include "LoginProvider.generated.h"

UINTERFACE(MinimalAPI)
class ULoginProvider : public UInterface
{
    GENERATED_BODY()
};
class ILoginProvider
{
    GENERATED_BODY()

public:
    // Inicializar el proveedor de login
    virtual void Initialize() = 0;

    // Intentar login con usuario y contraseña (si aplica)
    virtual bool Login(const FString& Username, const FString& Password) = 0;

    // Logout
    virtual void Logout() = 0;

    // Comprobar si está logueado
    virtual bool IsLoggedIn() const = 0;
};
