import * as React from "react";
import { Dispatch, SetStateAction } from "react";

/** Обёртка для манипуляций со стейтом функциональных компонентов приложения */
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

/** Обёртка для манипуляций со стейтом функционала работы с дампами */
export class DumpStateWrapper {
    static state: number = 0;
    static setState = (state: number) => {
        DumpStateWrapper.state = state;
    }
    static getState = () => DumpStateWrapper.state;
}
