import * as React from 'react';
import {LoaderComponent} from "./loader.component.tsx";

interface IState {
    data: any;
    time: any;
    menuListener: any;
}

interface IProps {
    subscription: any;
    formId: any;
    jsonStorage: any;
    id: any;
}

class CreateView extends React.Component<IProps, IState> {
    formId: any;
    mounted: boolean;

    public state: IState = {
        data: null,
        // NB использовать реальное время более корректно для key:
        time: null,
        menuListener: null
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
        LoaderComponent.unusedPromise = LoaderComponent.getData(this, LoaderComponent.createUrl);
    }

    componentWillUnmount() {
        // переход на update при несохраненной заметке не приведёт к ошибке 400 (сервер не понимает NaN):
        if (isNaN(window.textId)) {
            window.textId = 0;
        }
        this.mounted = false;
    }

    componentDidUpdate() {
        if (this.state.menuListener){
            console.log("Redirected: " + this.state.menuListener);
            LoaderComponent.redirectToMenu("/#/read/" + this.state.menuListener);
        }

        let id = 0;
        if (this.state.data) id = Number(this.state.data.savedTextId);
        if (id !== 0) window.textId = id;
    }

    render() {
        let checkboxes = [];
        if (this.state.data != null && this.state.data.genresNamesCS != null) {
            for (let i = 0; i < this.state.data.genresNamesCS.length; i++) {
                checkboxes.push(<Checkbox key={`checkbox ${i}${this.state.time}`} id={i} jsonStorage={this.state.data} subscription={null} formId={null}/>);
            }
        }

        let jsonStorage = this.state.data;
        if (jsonStorage) {
            if (!jsonStorage.textCS) jsonStorage.textCS = "";
            if (!jsonStorage.titleCS) jsonStorage.titleCS = "";
        }

        return (
            <div id="renderContainer">
                <form ref={this.mainForm}
                    id="dizzy">
                    {checkboxes}
                    {this.state.data != null &&
                        <SubmitButton subscription={this} formId={this.formId} id={null} jsonStorage={null}/>
                    }
                </form>
                {this.state.data != null &&
                    <Message formId={this.formId} jsonStorage={jsonStorage} subscription={null} id={null}/>
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

        let getGenreId = (i: number) => {
            if (this.props.jsonStorage.genresNamesId !== undefined) {
                return this.props.jsonStorage.genresNamesId[i];
            }
            else {
                return "";
            }
        };

        return (
            <div id="checkboxStyle">
                <input name="chkButton" value={this.props.id} type="checkbox" id={this.props.id} className="regular-checkbox" defaultChecked={checked} />
                <label htmlFor={this.props.id} onClick={SubmitButton.loadNoteOnClick} about={getGenreId(this.props.id)}>
                    {getGenreName(this.props.id)}
                </label>
            </div>
        );
    }
}

class Message extends React.Component<IProps> {
    textHandler = (e: any) => {
        this.props.jsonStorage.textCS = e.target.value;
        this.forceUpdate();
    }

    titleHandler = (e: any) => {
        this.props.jsonStorage.titleCS = e.target.value;
        this.forceUpdate();
    }

    render() {
        return (
            <div >
                <p />
                {this.props.jsonStorage != null ?
                    <div>
                        <h5>
                            <textarea name="ttl" cols={66} rows={1} form="dizzy"
                                value={this.props.jsonStorage.titleCS} onChange={this.titleHandler} />
                        </h5>
                        <h5>
                            <textarea name="msg" cols={66} rows={30} form="dizzy"
                                value={this.props.jsonStorage.textCS} onChange={this.textHandler} />
                        </h5>
                    </div>
                    : "loading.."}
            </div>
        );
    }
}

class SubmitButton extends React.Component<IProps> {

    requestBody: any;
    storage: string[] = [];
    storageId: string[] = [];
    submitState: any;
    btn: any;
    static state: any;
    static listener: any;

    constructor(props: any) {
        super(props);
        this.submit = this.submit.bind(this);
        // подтверждение или отмена:
        this.submitState = 0;
        SubmitButton.listener = this.props.subscription;
    }

    // чекбоксы превращаются в ссылки на каталог заметок:
    static loadNoteOnClick: any = (e: any) => {
        if (SubmitButton.state !== undefined) {

            let title = e.target.innerText;
            let id = e.target.attributes.about.nodeValue;

            // subscription на компонент create.
            console.log("Submitted: " + SubmitButton.state + " " + title + " " + id);
            SubmitButton.listener.setState({menuListener: id});
        }
    }

    cancel = (e: any) => {
        e.preventDefault();
        this.btn.style.display = "none";
        // отмена - сохраняем текст и название:
        SubmitButton.state = undefined;
        this.submitState = 0;

        let text = JSON.parse(this.requestBody).TextJS;
        let title = JSON.parse(this.requestBody).TitleJS;

        this.requestBody = JSON.stringify({
            "CheckedCheckboxesJS":[],
            "TextJS": text,
            "TitleJS": title
            });
        LoaderComponent.unusedPromise = LoaderComponent.postData(this.props.subscription, this.requestBody, LoaderComponent.createUrl);
    }

    componentDidMount() {
        this.btn  = (document.getElementById('cancelButton') as HTMLInputElement);
        this.btn.style.display = "none";
    }

    async submit(e: any) {
        e.preventDefault();

        this.btn.style.display = "none";
        SubmitButton.state = this.submitState;

        if (this.submitState === 1)
        {
            // подтверждение:
            SubmitButton.state = undefined;

            this.submitState = 0;
            LoaderComponent.unusedPromise = LoaderComponent.postData(this.props.subscription, this.requestBody, LoaderComponent.createUrl);
            return;
        }

        let formData = new FormData(this.props.formId);
        let checkboxesArray = (formData.getAll('chkButton')).map(a => Number(a) + 1);
        let formMessage = formData.get('msg');
        let formTitle = formData.get('ttl');
        const item = {
            CheckedCheckboxesJS: checkboxesArray,
            TextJS: formMessage,
            TitleJS: formTitle
        };
        this.requestBody = JSON.stringify(item);

        this.storage = [];
        let promise = this.findSimilarNotes(formMessage, formTitle);
        await promise;

        if (this.storage.length > 0)
        {
            // переключение в "подтверждение или отмена":
            this.btn.style.display = "block";
            this.submitState = 1;
            return;
        }

        // совпадения не обнаружены:
        LoaderComponent.unusedPromise = LoaderComponent.postData(this.props.subscription, this.requestBody, LoaderComponent.createUrl);
    }

    findSimilarNotes = async (formMessage: string | File | null, formTitle: string | File | null) => {
        let promise;

        if (typeof formMessage === "string") {
            formMessage = formMessage.replace(/\r\n|\r|\n/g, " ");
        }

        let callback = (data: any) => this.getNoteTitles(data);
        let query = "?text=" + formMessage + " " + formTitle;

        try {
            promise = LoaderComponent.getWithPromise(LoaderComponent.findUrl, query, callback);
        } catch (err) {
            console.log("Find when create: try-catch err");
        }

        if (promise !== undefined) {
            await promise;}
    }

    getNoteTitles = async (res: any) => {
        let result = [];
        let response = res['res'];
        if (response === undefined) {
            return;
        }

        let array = Object.keys(response).map((key) => [Number(key), response[key]]);
        array.sort(function (a, b) {
            return b[1] - a[1]
        });

        for (let index in array) {
            result.push(array[index]);
        }

        if (result.length === 0) {
            return;
        }

        for (let ind = 0; ind < result.length; ind++) {
            // лучше сделать reject:
            if (this.storage.length >= 10) {
                continue;
            }

            let i = String(result[ind][0]);

            // получаем имена возможных совпадений: i:string зто id заметки, можно вместо time его выставлять:
            let promise;

            let callback = (data: any) => this.getTitle(data, i);

            let query = "?id=" + i;

            try {
                promise = LoaderComponent.getWithPromise(LoaderComponent.readTitleUrl, query, callback);
            } catch (err) {
                console.log("Find when create: try-catch err");
            }

            if (promise !== undefined) {
                await promise;
            }
        }
    }

    getTitle = (data: any, i: string) => {
        this.storage.push((data.res + '\r\n'));
        this.storageId.push(i);

        let time = Date.now();
        // stub:
        data = {
            "genresNamesCS": this.storage,
            "isGenreCheckedCS": [],
            "textCS": JSON.parse(this.requestBody).TextJS,
            "titleCS": JSON.parse(this.requestBody).TitleJS,
            "genresNamesId": this.storageId
        };
        // subscription на CreateView:
        this.props.subscription.setState({ data , time });
    }

    componentWillUnmount() {
        // TODO отменить подписки и асинхронную загрузку
    }

    render() {
        return (
            <div id="submitStyle">
                <input type="checkbox" id="submitButton" className="regular-checkbox" />
                <label htmlFor="submitButton" onClick={this.submit}>Создать</label>
                  <div id="cancelButton">
                    <input type="checkbox" id="submitButton" className="regular-checkbox" />
                    <label htmlFor="submitButton" onClick={this.cancel}>Отменить</label>
                  </div>
            </div>
        );
    }
}

export default CreateView;
