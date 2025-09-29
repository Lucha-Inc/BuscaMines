#include "User/CacoUser.h"
#include "User/CacoUserStruct.h"

UCacoUser::UCacoUser() {
    // Constructor vacío
}


void UCacoUser::Init(const FCacoUserStruct& InStruct) {
    UserId = InStruct.UserId;
    DisplayName = InStruct.DisplayName;
}

UCacoUser* UCacoUser::CreateUserFromStruct(UObject* Outer, const FCacoUserStruct& InStruct) {
    UCacoUser* NewUser = NewObject<UCacoUser>(Outer);
    if (NewUser) {
        NewUser->Init(InStruct);
    }
    return NewUser;
}

FString UCacoUser::GetUserId() const {
    return UserId;
}

FString UCacoUser::GetDisplayName() const {
    return DisplayName;
}
