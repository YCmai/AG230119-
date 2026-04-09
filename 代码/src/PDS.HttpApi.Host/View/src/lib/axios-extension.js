import axios from "axios";
const service = axios.create({
    timeout: 30000,
});
service.interceptors.request.use(request => {
    // let language = store.get;
    return request;
});

service.interceptors.response.use(response => {
    if (response.status == 200 || response.status == 204) {
        return Promise.resolve(response.data);
    }
}, error => {
    if (error.response && error.response.data && error.response.data.error)
        return Promise.reject(error.response.data.error);
    else if (error.response)
        return Promise.reject({ code: error.response.status, message: error.response.statusText });
    else
        return Promise.reject({ code: "UnknowError", message: "UnknowMessage" });
});

export default service;