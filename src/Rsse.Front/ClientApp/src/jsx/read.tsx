﻿import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Loader } from "./loader";
import { hideMenu } from "./hideMenu";

interface IState {
    data: any;
}
interface IProps {
    listener: any;
    formId: any;
    jsonStorage: any;
    id: any;
}

export class HomeView extends React.Component<IState> {
    formId: any;
    mounted: boolean;

    public state: IState = {
        data: null
    }

    mainForm: React.RefObject<HTMLFormElement>;

    constructor(props: any) {
        super(props);
        this.formId = null;
        this.mounted = true;

        this.mainForm = React.createRef();
    }

    componentDidMount() {
        this.formId = this.mainForm.current;
        Loader.getData(this, Loader.readUrl);
    }

    componentDidUpdate() {
        ReactDOM.render(
            <div>
                <SubmitButton listener={this} formId={this.formId} jsonStorage={null} id={null}/>
            </div>,
            document.querySelector("#searchButton1")
        );
        // внешняя зависимость
        (document.getElementById("header")as HTMLElement).style.backgroundColor = "#e9ecee";//???
    }

    componentWillUnmount() {
        this.mounted = false;
    }

    render() {
        let checkboxes = [];
        if (this.state.data != null) {
            for (let i = 0; i < this.state.data.genresNamesCS.length; i++) {
                checkboxes.push(<Checkbox key={`checkbox ${i}`} id={i} jsonStorage={this.state.data} listener={null} formId={null}/>);
            }
        }

        return (
            <div>
                
                <form ref={this.mainForm}
                    id="dizzy">
                    {checkboxes}
                </form>
                <div id="messageBox">
                    {this.state.data != null && this.state.data.textCS != null &&
                        <Message formId={this.formId} jsonStorage={this.state.data} listener={null} id={null}/>
                    }
                </div>
            </div>
        );
    }
}

class Checkbox extends React.Component<IProps> {
    render() {
        let getGenreName = (i: number) => {
            return this.props.jsonStorage.genresNamesCS[i];
        };
        return (
            <div id="checkboxStyle">
                <input name="chkButton" value={this.props.id} type="checkbox" id={this.props.id} className="regular-checkbox" defaultChecked={false} />
                <label htmlFor={this.props.id}>{getGenreName(this.props.id)}</label>
            </div>
        );
    }
}

interface IWithLinks{
    text: string;
}

class WithLinks extends React.Component<IWithLinks> {
    
    constructor(props: any) {
        super(props);
    }
    
    render() {
        let res: any = [];

        // https://css-tricks.com/almanac/properties/o/overflow-wrap/#:~:text=overflow%2Dwrap%20is%20generally%20used,%2C%20and%20Korean%20(CJK).
        // [TODO] такую ссылку парсит некорректно, съедает ).
        this.props.text && this.props.text.replace(
            /((?:https?:\/\/|ftps?:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,})|(\n+|(?:(?!(?:https?:\/\/|ftp:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,}).)+)/gim, 
            (m, link, text) => {
            return res.push(link ? <a href={(link[0]==="w" ? "//" : "") + link} key={res.length}>{link}</a> : text);
        })

        return <div className="user-text">{res}</div>
    }
}

class Message extends React.Component<IProps> {
    constructor(props: any) {
        super(props);
        this.hideMenu = this.hideMenu.bind(this);
    }
    
    hideMenu() {
        this.props.formId.style.display = hideMenu(this.props.formId.style.display);
    }

    render() {
        if (this.props.jsonStorage && Number(this.props.jsonStorage.savedTextId) !== 0) {
            window.textId = Number(this.props.jsonStorage.savedTextId);
        }
        
        return (
            <span>
                {this.props.jsonStorage != null ? (this.props.jsonStorage.textCS != null ?
                    <span>
                        <div id="songTitle" onClick={this.hideMenu}>
                            {this.props.jsonStorage.titleCS}
                        </div>
                        <div id="songBody">
                                {/* {this.props.jsonStorage.textCS} */}
                            <WithLinks text={this.props.jsonStorage.textCS} />
                        </div>
                    </span>
                    : "выберите жанр")
                    : ""}
            </span>
        );
    }
}

class SubmitButton extends React.Component<IProps> {

    constructor(props: any) {
        super(props);
        this.submit = this.submit.bind(this);
    }

    submit(e: any) {
        e.preventDefault();
        // внешняя зависимость
        (document.getElementById("login") as HTMLElement).style.display = "none";
        let formData = new FormData(this.props.formId);
        let checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        const item = {
            CheckedCheckboxesJS: checkboxesArray
        };
        let requestBody = JSON.stringify(item);
        Loader.postData(this.props.listener, requestBody, Loader.readUrl);
        // внешняя зависимость
        (document.getElementById("header") as HTMLElement).style.backgroundColor = "slategrey"; // #4cff00
    }

    componentWillUnmount() {
        // отменяй подписки и асинхронную загрузку
    }

    render() {
        return (
            <div id="submitStyle">
                <input form="dizzy" type="checkbox" id="submitButton" className="regular-checkbox" onClick={this.submit} />
                <label htmlFor="submitButton">Поиск</label>
            </div>
        );
    }
}