import * as React from "react";
import { Dispatch, SetStateAction } from "react";

/** Обёртка, позволяющая загрузчику работать со стейтом функциональных компонентов приложения */
export class FunctionComponentStateWrapper<T> {
    public mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>];
    public setData: Dispatch<SetStateAction<T|null>>;
    public data: T|null;
    constructor(mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>],
                setData: Dispatch<SetStateAction<T|null>>,
                data: T|null) {
        this.mounted = mounted;
        this.setData = setData;
        this.data = data;
    }
}

/** Синглтон, инициализирующий некоторое состояние независимо от компонента */
export class StateStorageWrapper {
    static state: number = 0;
    static displayed: boolean = false;
    static gotNoteById: boolean = false;

    static setState = (state: number) => {
        StateStorageWrapper.state = state;
    }
    static getState = () => StateStorageWrapper.state;
}
