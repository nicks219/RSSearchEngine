﻿import {
    FunctionComponentStateWrapper,
    RecoveryStateStorage,
    StateTypesAlias
} from "./state.handlers";

/** Изменить видимость контейнера с меню на противоположную, см. LoginBoxSetVisibility и hideMenu */
export const toggleMenuVisibility = (cssProperty: string): string => {
    if (cssProperty !== "none") {
        cssProperty = "none";
        (document.getElementById("login") as HTMLElement).style.display = "none";
        return cssProperty;
    }

    cssProperty = "block";
    return cssProperty;
}


/** Функционал изменения видимости контейнера с логином.
* При проявлении компонента необходимо сохранить контест восстановления. */
export const LoginBoxVisibility = (
    visibility: boolean,
    stateWrapper?: FunctionComponentStateWrapper<StateTypesAlias>,
    url?: string,
    recoveryContext?: RecoveryStateStorage<StateTypesAlias>) => {

    const SetInvisible = () => {
        const loginMessage = document.getElementById("loginMessage") as HTMLElement;
        if (loginMessage) {
            loginMessage.style.display = "none";
        }
    }

    const SetVisible = () => {
        if (recoveryContext && url && stateWrapper) {
            recoveryContext.recoveryStateWrapper = stateWrapper as unknown as FunctionComponentStateWrapper<StateTypesAlias>;
            recoveryContext.recoveryString = url;
        }

        const loginMessageElement = document.getElementById("loginMessage") as HTMLElement;
        const loginElement = document.getElementById("login") as HTMLElement;
        loginMessageElement.style.display = "block";
        loginElement.style.display = "block";
        // изменение css только в dom браузера, при этом стейт компонента останется submitStyleGreen:
        const submitElement = loginElement.children[0];
        submitElement.id = "submitStyle";
    }

    if (visibility) {
        SetVisible();
    } else if (!visibility) {
        SetInvisible();
    }
}
