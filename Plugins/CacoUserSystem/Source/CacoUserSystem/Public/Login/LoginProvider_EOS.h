// LoginProvider_EOS.h
#pragma once

#include "CoreMinimal.h"
#include "Login/LoginProvider.h"
//#include "OnlineSubsystemEOS.h"
#include "Interfaces/OnlineIdentityInterface.h"
#include "LoginProvider_EOS.generated.h"

UCLASS()
class CACOUSERSYSTEM_API ULoginProvider_EOS : public UObject, public ILoginProvider
{
    GENERATED_BODY()

public:
    virtual void Initialize() override;
    virtual bool Login(const FString& Username, const FString& Password) override;
    virtual void Logout() override;
    virtual bool IsLoggedIn() const override;

//private:
    IOnlineIdentityPtr IdentityInterface;
};
