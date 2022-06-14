import * as React from 'react';
import {Loader} from "./loader";
import { hideMenu } from "./hideMenu";

interface IState {
    data: any;
    time: any;
}

interface IProps {
    listener: any;
    formId: any;
    jsonStorage: any;
    id: any;
}

class UpdateView extends React.Component<IProps, IState> {
    formId: any;
    mounted: boolean;

    public state: IState = {
        data: null,
        time: null
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
        Loader.getDataById(this, window.textId, Loader.updateUrl);
    }

    componentWillUnmount() {
        this.mounted = false;
    }

    render() {
        let checkboxes = [];
        if (this.state != null && this.state.data != null) {
            for (let i = 0; i < this.state.data.genresNamesCS.length; i++) {
                checkboxes.push(<Checkbox key={"checkbox " + i + this.state.time} id={i} jsonStorage={this.state.data} listener={null} formId={null}/>);
            }
        }

        return (
            <div>
                <form ref={this.mainForm} id="dizzy">
                    {checkboxes}
                    {this.state != null && this.state.data != null &&
                        <SubmitButton listener={this} formId={this.formId} jsonStorage={this.state.data} id={null}/>
                    }
                </form>
                {this.state != null && this.state.data != null && this.state.data.textCS != null &&
                    <Message formId={this.formId} jsonStorage={this.state.data} listener={null} id={null}/>
                }
            </div>
        );
    }
}

class Checkbox extends React.Component<IProps> {
    
    render() {
        let checked = this.props.jsonStorage.isGenreCheckedCS[this.props.id] === "checked";
        let getGenreName = (i: number) => {
            return this.props.jsonStorage.genresNamesCS[i];
        };
        return (
            <div id="checkboxStyle">
                <input name="chkButton" value={this.props.id} type="checkbox" id={this.props.id} className="regular-checkbox"
                    defaultChecked={checked} />
                <label htmlFor={this.props.id}>{getGenreName(this.props.id)}</label>
            </div>
        );
    }
}

class Message extends React.Component<IProps> {
    constructor(props: any) {
        super(props);
        this.hideMenu = this.hideMenu.bind(this);
    }

    componentDidMount() {
        this.getCookie();
    }

    // куки сервиса: .AspNetCore.Cookies
    getCookie = () => {
        // куки выставляются в компоненте Login
        const name = "rsse_auth";
        let matches = document.cookie.match(new RegExp(
            "(?:^|; )" + name.replace(/([.$?*|{}()\[\]\\\/+^])/g, '\\$1') + "=([^;]*)"
        ));

        // return matches ? decodeURIComponent(matches[1]) : undefined;
        if (matches == null || decodeURIComponent(matches[1]) === 'false')
        {
            this.hideMenu();
        }
    }
    
    hideMenu() {
        this.props.formId.style.display = hideMenu(this.props.formId.style.display);
    }

    inputText = (e: any) => {
        this.props.jsonStorage.textCS = e.target.value;
        this.forceUpdate();
    }

    render() {
        return (
            <div >
                <p />
                {this.props.jsonStorage != null ? (this.props.jsonStorage.textCS != null ?
                    <div>
                        <h1 onClick={this.hideMenu}>
                            {this.props.jsonStorage.titleCS}
                        </h1>
                        <h5>
                            <textarea name="msg" cols={66} rows={30} form="dizzy"
                                value={this.props.jsonStorage.textCS} onChange={this.inputText} />
                        </h5>
                    </div>
                    : "выберите песню")
                    : "loading.."}
            </div>
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
        let formData = new FormData(this.props.formId);
        let checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        let formMessage = formData.get("msg");
        const item = {
            CheckedCheckboxesJS: checkboxesArray,
            TextJS: formMessage,
            TitleJS: this.props.jsonStorage.titleCS,
            SavedTextId: window.textId
        };
        let requestBody = JSON.stringify(item);
        Loader.postData(this.props.listener, requestBody, Loader.updateUrl);
    }

    componentWillUnmount() {
        // отменяй подписки и асинхронную загрузку
    }

    render() {
        return (
            <div id="submitStyle">
                <input type="checkbox" id="submitButton" className="regular-checkbox" onClick={this.submit} />
                <label htmlFor="submitButton">Сохранить</label>
            </div>
        );
    }
}

export default UpdateView;