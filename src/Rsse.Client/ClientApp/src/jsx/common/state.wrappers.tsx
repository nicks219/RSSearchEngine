import * as React from "react";
import { Dispatch, SetStateAction } from "react";
import {IDataTimeState} from "../components/update.component.tsx";

/** Обёртка, позволяющая загрузчику работать со стейтом функциональных компонентов приложения */
export class FunctionComponentStateWrapper<T> {
    public mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>];
    public setData: Dispatch<SetStateAction<T|null>>|null;
    public data: T|null;// <= где используется это поле?
    public setComplexData?: Dispatch<SetStateAction<IDataTimeState|null>>;// <= предопределенный тип

    constructor(mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>],
                setData: Dispatch<SetStateAction<T|null>>|null,
                data: T|null,// <= где используется это поле?
                setComplexData?: Dispatch<SetStateAction<IDataTimeState|null>>) {
        this.mounted = mounted;
        this.setData = setData;
        this.data = data;
        this.setComplexData = setComplexData;
    }
}

/** Синглтон, инициализирующий некоторое состояние независимо от компонента */
export class StateStorageWrapper {
    static state: number = 0;
    static renderedAfterRedirect: boolean = false;
    static redirectCall: boolean = false;

    static setState = (state: number) => {
        StateStorageWrapper.state = state;
    }
    static getState = () => StateStorageWrapper.state;
}
