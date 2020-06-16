var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
define("shared/utils", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function clone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }
    exports.clone = clone;
    function spinner(show) {
        const spinnerObj = document.getElementById("spinner");
        if (!spinnerObj) {
            throw Error("Could not find a spinner object");
        }
        spinnerObj.style.display = show ? "block" : "none";
    }
    exports.spinner = spinner;
    function dynamicEventHook(events, selector, handler) {
        if (!Array.isArray(events)) {
            events = [events];
        }
        for (const event of events) {
            document.addEventListener(event, e => {
                if (e.target && Array.from(document.querySelectorAll(selector)).indexOf(e.target) !== -1) {
                    handler.call(e.target, e);
                }
            });
        }
    }
    exports.dynamicEventHook = dynamicEventHook;
    function toCamelCase(s) {
        return s.replace(/([-_][a-z])/ig, ($1) => {
            return $1.toUpperCase()
                .replace("-", "")
                .replace("_", "");
        });
    }
    exports.toCamelCase = toCamelCase;
    function urlBase64ToUint8Array(base64String) {
        const padding = "=".repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/\-/g, "+")
            .replace(/_/g, "/");
        const rawData = window.atob(base64);
        return Uint8Array.from([...rawData].map(char => char.charCodeAt(0)));
    }
    exports.urlBase64ToUint8Array = urlBase64ToUint8Array;
});
define("shared/ajax", ["require", "exports", "shared/utils"], function (require, exports, utils_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    let csrfToken;
    function init(token) {
        csrfToken = token;
    }
    exports.init = init;
    function ajaxGet(url, queryString, onSuccess, onErrorResponse, onFailure) {
        ajax("GET", url, queryString, undefined, onSuccess, onErrorResponse, onFailure);
    }
    exports.ajaxGet = ajaxGet;
    function ajaxPost(url, body, onSuccess, onErrorResponse, onFailure) {
        ajax("POST", url, undefined, body, onSuccess, onErrorResponse, onFailure);
    }
    exports.ajaxPost = ajaxPost;
    function ajax(method, url, queryString, body, onSuccess, onErrorResponse, onFailure) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            utils_1.spinner(true);
            const headers = body ? { "Content-Type": "application/json" } : {};
            if (method === "POST") {
                headers["X-CSRF-TOKEN"] = csrfToken;
            }
            try {
                const requestInit = { method, headers };
                if (body) {
                    requestInit.body = JSON.stringify(body);
                }
                const response = yield fetch(url + (queryString || ""), requestInit);
                utils_1.spinner(false);
                if (response.ok) {
                    const value = yield response.text();
                    (_a = onSuccess) === null || _a === void 0 ? void 0 : _a(value);
                }
                else {
                    const errorHandler = (onErrorResponse !== null && onErrorResponse !== void 0 ? onErrorResponse : defaultAjaxOnErrorResponse);
                    errorHandler(response);
                }
            }
            catch (error) {
                utils_1.spinner(false);
                const errorHandler = (onFailure || defaultAjaxOnFailure);
                errorHandler(error);
            }
        });
    }
    function queryStringFromObject(obj) {
        if (!obj) {
            return "";
        }
        let query = Object
            .keys(obj)
            .filter(k => !!obj[k])
            .map(k => encodeURIComponent(k) + "=" + encodeURIComponent(obj[k]))
            .join("&");
        if (query) {
            query = "?" + query;
        }
        return query;
    }
    exports.queryStringFromObject = queryStringFromObject;
    function defaultAjaxOnErrorResponse(response) {
        alert(`Error returned by server:\r\n${response.status} - ${response.statusText}`);
    }
    exports.defaultAjaxOnErrorResponse = defaultAjaxOnErrorResponse;
    function defaultAjaxOnFailure(error) {
        alert(error);
    }
});
define("app", ["require", "exports", "shared/ajax"], function (require, exports, ajax_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function init(csrfToken) {
        ajax_1.init(csrfToken);
        initLogInLogOutLinks();
    }
    exports.init = init;
    function initLogInLogOutLinks() {
        const elements = Array.from(document.getElementsByClassName("js-add-return-url"));
        for (const element of elements) {
            element.addEventListener("click", e => {
                const target = e.target;
                window.location.href = `${target.href}?returnUrl=${encodeURIComponent(document.location.href)}`;
                return false;
            });
        }
    }
});
define("shared/modal", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    const modal = (() => {
        let currentModal;
        let onClose;
        function modalContainer() {
            const container = document.getElementById("modal-container");
            if (container) {
                return container;
            }
            throw new Error("Could not find container for modal");
        }
        function showModal(title, contents, closeCallback) {
            hideModal();
            const modalHtml = `<div class="modal-header">
                <h5 class="modal-title">${title}</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">Г—</span>
                </button>
            </div>
            <div class="modal-body" id="modal-body">
                ${contents}
            </div>`;
            currentModal = new Modal(modalContainer(), { content: modalHtml });
            onClose = closeCallback;
            modalContainer().addEventListener("hidden.bs.modal", _ => {
                var _a;
                (_a = onClose) === null || _a === void 0 ? void 0 : _a();
                currentModal = undefined;
                onClose = undefined;
            }, false);
            currentModal.show();
        }
        function modalContents() {
            modalContainer().querySelector(".modal-body");
        }
        function hideModal() {
            var _a;
            (_a = currentModal) === null || _a === void 0 ? void 0 : _a.hide();
        }
        return {
            show: showModal,
            hide: hideModal,
            contents: modalContents
        };
    })();
    exports.default = modal;
});
define("strings/string-edit", ["require", "exports", "shared/ajax", "shared/utils", "shared/modal"], function (require, exports, ajax_2, utils_2, modal_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function showString(stringId, reuseModal) {
        ajax_2.ajaxGet("/string_edit_ui", ajax_2.queryStringFromObject({ stringId }), html => {
            if (reuseModal) {
                const content = document.getElementById("modal-body");
                if (!content) {
                    throw Error("Couldn't find the modal-body element");
                }
                content.innerHTML = html;
            }
            else {
                // when people visit the string url by doing a request, we don't need to pushState
                const newPath = `/strings/${stringId}`;
                if (window.location.pathname !== newPath) {
                    history.pushState({ stringId, prevUrl: window.location.href, prevState: history.state }, "", newPath);
                }
                modal_1.default.show("Suggestions", html, () => {
                    var _a, _b, _c;
                    history.pushState((_a = history.state) === null || _a === void 0 ? void 0 : _a.prevState, "", (_c = (_b = history.state) === null || _b === void 0 ? void 0 : _b.prevUrl, (_c !== null && _c !== void 0 ? _c : "/")));
                });
            }
        });
    }
    exports.showString = showString;
    function init() {
        const suggestionCreationErrors = {
            2: "We couldn't find the id you send, did you need to refresh your page?",
            3: "The suggestion you are sending is the same as the actual translation",
            4: "You sent an empty suggestion, please try to send a suggestion next time",
            5: "The suggestion you are sending is already suggested. Maybe you need to refresh?",
            6: "Failed sending the suggestion. You are missing some variables",
            7: "Failed sending the suggestion. You have included unrecognized variables",
            8: "A database error has ocurred, please try again."
        };
        utils_2.dynamicEventHook("click", ".string-list tr td:first-of-type", (e) => {
            const tgt = e.target;
            if (tgt && tgt.parentElement) {
                loadStringEditorFor(tgt.parentElement);
            }
        });
        function loadStringEditorFor(searchResultsRow) {
            const stringId = searchResultsRow.getAttribute("data-string-id");
            if (!stringId) {
                throw Error("Could not find the string id to load");
            }
            showString(stringId);
        }
        utils_2.dynamicEventHook("click", "button[data-string-action]", (e) => {
            executeStringActionFor(e.target);
        });
        // TODO: Find a better way to hook the thumb up/down icon clicks
        utils_2.dynamicEventHook("click", "button[data-string-action] *", (e) => {
            if (e.target) {
                const tgt = e.target;
                const action = tgt.closest("[data-string-action]");
                if (!action) {
                    throw Error("Could not find the action to perform");
                }
                executeStringActionFor(action);
            }
        });
        utils_2.dynamicEventHook(["change", "keyup", "paste"], "textarea[data-string-action]", (e) => {
            if (e.target) {
                executeStringActionFor(e.target);
            }
        });
        function executeStringActionFor(element) {
            const stringContainer = element.closest("[data-string-id]");
            if (!stringContainer) {
                throw Error("Could not find the string id container");
            }
            const stringId = stringContainer.getAttribute("data-string-id");
            if (!stringId) {
                throw Error("Could not find the string id");
            }
            const actionNameAttr = element.getAttribute("data-string-action");
            if (!actionNameAttr) {
                throw Error("Could not find the action name attribute");
            }
            const actionName = utils_2.toCamelCase(actionNameAttr);
            const action = getAction(actionName);
            action(stringId, element);
        }
        function getAction(actionName) {
            switch (actionName) {
                case "copyAsSuggestion": return (_, button) => {
                    const buttonTargetId = button.getAttribute("data-string-action-target");
                    if (!buttonTargetId) {
                        throw Error("Could not find the target id");
                    }
                    const target = document.getElementById(buttonTargetId);
                    if (!target) {
                        throw Error("Could not find the target");
                    }
                    const text = target.innerText;
                    const suggestionBox = document.getElementById("suggestion");
                    suggestionBox.value = text;
                };
                case "manageIgnore": return (stringId, button) => {
                    const doIgnore = button.getAttribute("data-string-action-argument") === "ignore";
                    ajax_2.ajaxPost("/manage-ignore", {
                        stringId: parseInt(stringId, 10),
                        ignored: doIgnore
                    }, text => {
                        const stringContainer = button.closest("[data-string-id]");
                        if (!stringContainer) {
                            throw Error("Could not find the string id container");
                        }
                        stringContainer.outerHTML = text;
                    });
                };
                case "manageUrgency": return (stringId, button) => {
                    const mustBeUrgent = button.getAttribute("data-string-action-argument") === "make-urgent";
                    ajaxForStringAction(stringId, "/manage-urgency", {
                        stringId: parseInt(stringId, 10),
                        isUrgent: mustBeUrgent
                    }, undefined, true);
                };
                case "handleSuggestionTextChanged": return (_, element) => {
                    const textarea = element;
                    const replaceButtons = document.querySelectorAll(".js-replace-suggestion");
                    for (const button of replaceButtons) {
                        button.disabled = !textarea.value.trim();
                    }
                };
                case "replaceSuggestion": return (stringId, button) => {
                    const suggestionElement = button.closest("[data-suggestion-id]");
                    if (!suggestionElement) {
                        throw Error("Could not find the suggestion");
                    }
                    const suggestionId = suggestionElement.getAttribute("data-suggestion-id");
                    if (!suggestionId) {
                        throw Error("Could not find suggestion id");
                    }
                    const newSuggestion = document.getElementById("suggestion").value.trim();
                    ajaxForStringAction(stringId, "/replace-suggestion", {
                        suggestionId: parseInt(suggestionId, 10),
                        newSuggestion
                    });
                };
                case "deleteSuggestion": return (stringId, button) => {
                    const suggestionElement = button.closest("[data-suggestion-id]");
                    if (!suggestionElement) {
                        throw Error("Could not find the suggestion");
                    }
                    const suggestionId = suggestionElement.getAttribute("data-suggestion-id");
                    if (!suggestionId) {
                        throw Error("Could not find the suggestion id");
                    }
                    ajaxForStringAction(stringId, "/delete-suggestion", {
                        suggestionId: parseInt(suggestionId, 10)
                    });
                };
                case "reviewSuggestion": return (stringId, button) => {
                    const suggestionElement = button.closest("[data-suggestion-id]");
                    if (!suggestionElement) {
                        throw Error("Could not find the suggestion");
                    }
                    const suggestionId = suggestionElement.getAttribute("data-suggestion-id");
                    if (!suggestionId) {
                        throw Error("Could not find the suggestion id");
                    }
                    const approve = button.getAttribute("data-review-action") === "approve";
                    ajaxForStringAction(stringId, "/review-suggestion", {
                        suggestionId: parseInt(suggestionId, 10),
                        approve
                    });
                };
                case "createSuggestion": return (stringId, button) => {
                    const rawStringCheckbox = document.getElementById("is-raw-string");
                    const suggestionElement = document.getElementById("suggestion");
                    const body = {
                        stringId: parseInt(stringId, 10),
                        suggestion: suggestionElement.value.trim(),
                        approve: button.getAttribute("data-create-approved-suggestion") === "yes",
                        rawString: rawStringCheckbox ? !!rawStringCheckbox.checked : false
                    };
                    ajaxForStringAction(stringId, "/create-suggestion", body, errorResponse => {
                        if (errorResponse.status !== 400) {
                            ajax_2.defaultAjaxOnErrorResponse(errorResponse);
                            return;
                        }
                        errorResponse.text().then(errorCode => {
                            var _a;
                            const errorMessage = (_a = suggestionCreationErrors[parseInt(errorCode, 10)], (_a !== null && _a !== void 0 ? _a : "The server encountered an error, but we don't know what happened"));
                            alert(errorMessage);
                        });
                    });
                };
            }
            throw Error("Unknown action: " + actionName);
        }
        function ajaxForStringAction(stringId, url, body, onErrorResponse, keepModalOpen) {
            ajax_2.ajaxPost(url, body, text => {
                const stringSummaryContainer = document.querySelector(`.js-string-summary[data-string-id='${stringId}']`);
                if (stringSummaryContainer) {
                    stringSummaryContainer.outerHTML = text;
                }
                if (keepModalOpen) {
                    showString(stringId, true);
                }
                else {
                    modal_1.default.hide();
                }
            }, onErrorResponse);
        }
    }
    exports.init = init;
});
define("strings/string-search", ["require", "exports", "shared/ajax", "shared/utils"], function (require, exports, ajax_3, utils_3) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function initializeStringSearch() {
        const queryDropdowns = document.querySelectorAll("select.js-string-query-filter");
        const queryTextInputs = document.querySelectorAll("input[type=text].js-string-query-filter");
        const queryLinks = document.querySelectorAll("a.js-string-query-filter");
        hookDropdowns();
        hookTextboxes();
        hookQuickLinks();
        hookHistoryPopState();
        function hookDropdowns() {
            for (const dropdown of queryDropdowns) {
                dropdown.onchange = e => { updateList(e.target, true); };
            }
        }
        function hookTextboxes() {
            let textInputTimeout;
            for (const input of queryTextInputs) {
                input.onkeyup = e => {
                    clearTimeout(textInputTimeout);
                    textInputTimeout = setTimeout(() => { updateList(e.target); }, 500);
                };
            }
        }
        function hookQuickLinks() {
            for (let i = 0; i < queryLinks.length; i++) {
                const link = queryLinks.item(i);
                link.onclick = e => {
                    const target = e.target;
                    const queryKey = target.getAttribute("data-string-query-key");
                    const queryValue = target.getAttribute("data-string-query-value");
                    if (queryValue == null) {
                        throw Error("Could not get a queryValue");
                    }
                    const dropdown = document.querySelector(`select[data-string-query-key=${queryKey}`);
                    if (!dropdown) {
                        throw Error("Could not find the dropdown");
                    }
                    dropdown.value = queryValue;
                    updateList(e.target, true);
                    e.preventDefault();
                };
            }
        }
        function hookHistoryPopState() {
            window.onpopstate = (_) => {
                location.reload();
            };
        }
        function updateList(triggeringElement, valueIsNumber) {
            var _a;
            utils_3.spinner(true);
            if (triggeringElement) {
                const queryKey = triggeringElement.getAttribute("data-string-query-key");
                if (!queryKey) {
                    throw Error("Could not find the queryKey");
                }
                // TODO: Check what things are passed here that have a value
                let value = null;
                const elementAsAny = triggeringElement;
                if (elementAsAny.value) {
                    value = elementAsAny.value;
                }
                let queryValue = (_a = triggeringElement.getAttribute("data-string-query-value"), (_a !== null && _a !== void 0 ? _a : value));
                if (valueIsNumber) {
                    queryValue = parseInt(queryValue, 10);
                }
                stringQueryFilters[queryKey] = queryValue;
            }
            const queryString = ajax_3.queryStringFromObject(stringQueryFilters);
            if (triggeringElement) {
                history.pushState(utils_3.clone(stringQueryFilters), "", queryString ? "filters" + queryString : "/");
            }
            ajax_3.ajaxGet("/strings_list", queryString, html => {
                const stringsList = document.getElementById("strings_list");
                if (!stringsList) {
                    throw Error("Could not get the strings list DOM element");
                }
                stringsList.innerHTML = html;
            });
        }
    }
    exports.default = initializeStringSearch;
});
define("strings/strings", ["require", "exports", "strings/string-search", "strings/string-edit", "strings/string-edit"], function (require, exports, string_search_1, string_edit_1, string_edit_2) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function init() {
        string_search_1.default();
        string_edit_1.init();
    }
    exports.init = init;
    exports.show = string_edit_2.showString;
});
define("users/notifications", ["require", "exports", "shared/ajax", "shared/utils"], function (require, exports, ajax_4, utils_4) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function run() {
        initializeNotifications();
    }
    exports.default = run;
    function initializeNotifications() {
        const supportsPush = ("serviceWorker" in navigator) && ("PushManager" in window);
        if (!supportsPush) {
            const container = document.getElementById("main-container");
            if (!container) {
                throw Error("Could not get the container DOM element");
            }
            container.outerHTML = "<div class='sorry-no-push'>Your browser doesn't support push notifications</div>";
            return;
        }
        document.querySelectorAll("[data-notification-name]").forEach(element => {
            element.addEventListener("click", e => {
                handleNotificationChange(e.target);
            });
        });
        function handleNotificationChange(element) {
            const notificationName = element.getAttribute("data-notification-name");
            if (!notificationName) {
                throw Error("Could not get the notification name");
            }
            notificationSettings[notificationName] = !notificationSettings[notificationName];
            if (notificationSettings[notificationName]) {
                element.classList.add("active");
            }
            else {
                element.classList.remove("active");
            }
        }
        const intervalValueSelector = document.getElementById("notifications-interval-value");
        intervalValueSelector.addEventListener("change", () => {
            notificationSettings.notificationsIntervalValue = parseInt(intervalValueSelector.value, 10);
        });
        const intervalSelector = document.getElementById("notifications-interval");
        intervalSelector.addEventListener("change", () => {
            notificationSettings.notificationsInterval = parseInt(intervalSelector.value, 10);
        });
        const saveButton = document.getElementById("save-and-add-browser");
        if (!saveButton) {
            throw Error("Could not find the save button");
        }
        saveButton.addEventListener("click", saveAndAddBrowser);
        const stopNotificationsButton = document.getElementById("stop-receiving-notifications");
        if (!stopNotificationsButton) {
            throw Error("Could not find the stop notifications button");
        }
        stopNotificationsButton.addEventListener("click", wipeNotifications);
        function saveAndAddBrowser() {
            return __awaiter(this, void 0, void 0, function* () {
                const subscription = yield subscribeUserToPush();
                ajax_4.ajaxPost("/users/me/notifications/update", { notifications: notificationSettings, subscription }, undefined, response => {
                    if (response.status === 401) {
                        history.pushState(null, "", "/");
                    }
                    else {
                        ajax_4.defaultAjaxOnErrorResponse(response);
                    }
                });
            });
        }
        function subscribeUserToPush() {
            return __awaiter(this, void 0, void 0, function* () {
                try {
                    yield navigator.serviceWorker.register("/service-worker.js");
                    const registration = yield navigator.serviceWorker.ready;
                    const subscribeOptions = {
                        applicationServerKey: utils_4.urlBase64ToUint8Array(notificationSettings.vapidPublic),
                        userVisibleOnly: true
                    };
                    return registration.pushManager.subscribe(subscribeOptions);
                }
                catch (e) {
                    alert("Error asking for permission: " + e.message);
                    throw e;
                }
            });
        }
        function wipeNotifications() {
            ajax_4.ajaxPost("/delete-notifications", {}, () => location.reload(), response => {
                if (response.status === 401) {
                    history.pushState(null, "", "/");
                }
                else {
                    ajax_4.defaultAjaxOnErrorResponse(response);
                }
            });
        }
    }
});
define("users/user-edit", ["require", "exports", "shared/ajax", "shared/utils"], function (require, exports, ajax_5, utils_5) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function run() {
        initializeUserEdit();
    }
    exports.default = run;
    function initializeUserEdit() {
        utils_5.dynamicEventHook("click", "[data-change-to-user-type]", (e) => {
            changeUserTypeFor(e.target);
        });
        function changeUserTypeFor(element) {
            const userSummaryContainer = element.closest("[data-user-id]");
            if (!userSummaryContainer) {
                throw Error("Could not get the summary container DOM element");
            }
            const userId = userSummaryContainer.getAttribute("data-user-id");
            if (!userId) {
                throw Error("Could not get the user Id");
            }
            const newUserType = element.getAttribute("data-change-to-user-type");
            if (!newUserType) {
                throw Error("Could not get the new user type");
            }
            ajax_5.ajaxPost("/users/change-type", {
                userId: parseInt(userId, 10),
                userType: parseInt(newUserType, 10)
            }, html => userSummaryContainer.outerHTML = html);
        }
    }
});
//# sourceMappingURL=app.js.map?v=8BF2E19E51400F16CBB755838521F6FFA9030B4C464242FEB9EB0678DAAD57A9
