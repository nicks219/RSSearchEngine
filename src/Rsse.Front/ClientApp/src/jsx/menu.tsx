import * as React from 'react';
import * as ReactDOM from 'react-dom';

import { HomeView } from "./read";
import UpdateView from "./update";
import CreateView from "./create";
import CatalogView from "./catalog";
import { Login } from "./login";

declare global {
    interface Window { textId: number }
}
window.textId = 0;

interface IProps {
    visibilityCss: string[];
    visibilityFlag: boolean[];
    menu: string[];
}

interface IState {
    id: number | any;
}

export default class Menu extends React.Component<IProps, IState> {
    visibilityCss: string[];
    visibilityFlag: boolean[];
    menu: string[];
    buttons: any;

    public static defaultProps: Partial<IProps> = {
        visibilityCss: ["act", "pas", "pas", "pas"],
        visibilityFlag: [true, false, false, false],
        menu: ["Посмотреть", "Поменять", "Создать", "Каталог"]
    }
    public state: IState = {
        id: null
    }

    constructor(a: IProps) {
        super(a);
        this.select = this.select.bind(this);
        this.visibilityCss = ["act", "pas", "pas", "pas"];
        this.visibilityFlag = [true, false, false, false];
        this.menu = ["Посмотреть", "Поменять", "Создать", "Каталог"];
    }

    select(e: any) {
        let target = Number(e.currentTarget.getAttribute('id').slice(5));
        // отмена update невыбранного материала
        if (target === 1 && window.textId === 0) return;

        this.visibilityFlag.forEach((value, index) => this.visibilityFlag[index] = false);
        this.visibilityFlag[target] = !this.visibilityFlag[target];
        this.forceUpdate(); // this.setState(this.state)
    }

    handleClick(event: any) {
        event.preventDefault();
        alert(event.currentTarget.tagName);
    }

    componentDidUpdate() {
        if (this.state.id) this.setState({id: null});
        if (!this.visibilityFlag[0]) {
            // костыль: убирает кнопку "Поиск"
            ReactDOM.render(
                <div>
                </div>,
                document.querySelector("#searchButton1")
            );
        }
    }

    render() {
        // костыль: переключаемся из catalog на changeText
        if (this.state.id) {
            this.visibilityFlag = [false, true, false, false];
            window.textId = this.state.id;
        }

        this.visibilityFlag.forEach((value, index) => this.visibilityFlag[index] ?
            this.visibilityCss[index] = "act" : this.visibilityCss[index] = "pas");

        this.buttons = [];
        for (let i = 0; i < 4; i++) {
            this.buttons.push(<button key={"menu " + i} onClick={this.select} id={"menu " + String(i)}
                                      className={this.visibilityCss[i]}
                // className="btn btn-info" style={{ margin: 10 + 'px' }}
            >
                {this.menu[i]}</button>);
        }

        // TODO: экран некрасиво мигает при переключении пунктов меню

        return (
            <div>
                <div id="header">
                    {this.buttons}
                </div>
                {/*первый пункт меню*/}
                {this.visibilityFlag[0] &&
                    <div id="renderContainer1">
                        <HomeView data={null}/>
                    </div>
                }
                {/*второй пункт меню*/}
                {this.visibilityFlag[1] &&
                    <div id="renderContainer">
                        <UpdateView listener={null} formId={null} id={null} jsonStorage={null}/>
                    </div>
                }
                {/*третий пункт меню*/}
                {this.visibilityFlag[2] &&
                    <div id="renderContainer">
                        <CreateView listener={this}  formId={null} id={null}  jsonStorage={null}/>
                    </div>
                }
                {/*четвертый пункт меню*/}
                {this.visibilityFlag[3] &&
                    <div id="renderContainer">
                        <CatalogView listener={this}/>
                    </div>
                }
            </div>
        );
    }
}

ReactDOM.render(
    <Login listener={this} formId={null} jsonStorage={null} id={null}/>
    , document.querySelector("#renderLoginForm")
);
