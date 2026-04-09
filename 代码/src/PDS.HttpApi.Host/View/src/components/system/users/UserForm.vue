<template>
  <a-modal
    ref="dialog"
    :width="650"
    :title="$t('title.user')"
    :visible="true"
    class="default-hb-modal"
  >
    <a-form-model :model="form" ref="userForm" layout="inline" :rules="rules">
      <a-form-model-item :label="$t('title.name')" prop="userName" class="hb-form-col-2">
        <a-input v-model="form.userName" :placeholder="$t('placeholder.name')"></a-input>
      </a-form-model-item>
      <a-form-model-item
        v-if="isCreat"
        :label="$t('title.password')"
        prop="password"
        class="hb-form-col-2"
      >
        <a-input-password
          v-model="form.password"
          :placeholder="$t('placeholder.password')"
        >
        </a-input-password>
      </a-form-model-item>
      <a-form-model-item
        :label="$t('title.roles')"
        prop="roleNames"
        class="hb-form-col-2"
      >
        <a-select
          v-model="form.roleNames"
          style="width: 175px"
          mode="multiple"
          @change="change"
          ref="roleNames"
        >
          <a-select-option v-for="(role, index) in roles" :key="index" :value="role.name">
            {{ role.name }}
          </a-select-option>
        </a-select>
      </a-form-model-item>
      <a-form-model-item :label="$t('title.email')" prop="email" class="hb-form-col-2">
        <a-input v-model="form.email" :placeholder="$t('placeholder.email')"></a-input>
      </a-form-model-item>

      <a-form-model-item
        :label="$t('title.phoneNumber')"
        prop="phoneNumber"
        class="hb-form-col-2"
      >
        <a-input v-model="form.phoneNumber" :placeholder="$t('placeholder.phoneNumber')">
        </a-input>
      </a-form-model-item>
      <a-form-model-item :label="$t('title.isActive')" class="hb-form-col-2">
        <a-checkbox v-model="form.isActive"></a-checkbox>
      </a-form-model-item>
    </a-form-model>
    <template slot="footer">
      <a-button key="back" @click="close" size="small">
        {{ $t("actions.cancel") }}
      </a-button>
      <a-button key="submit" type="primary" :loading="loading" @click="save" size="small">
        {{ $t("actions.save") }}
      </a-button>
    </template>
  </a-modal>
</template>
<script>
import component from "@/lib/base";
import locale from "@/localize/system/user";
import axios from "axios";
import { Get as GetUser, Add, Update, GetRoles as GetUserRoles } from "@/api/user";
import { GetAll as GetRoles } from "@/api/role";
let _this;
export default {
  name: "UserForm",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  data() {
    let validPhone = function (rule, value, callback) {
      if (!value) return callback();
      if (/^1([358][0-9]|4[579]|66|7[0135678]|9[89])[0-8]{8}$/.test(value))
        return callback();
      else return callback(new Error(_this.$t("valid.phone")));
    };
    return {
      loading: false,
      id: "0",
      form: {
        userName: "",
        email: "",
        phoneNumber: "",
        password: "",
        roleNames: [],
        isActive: true,
      },
      roles: [],
      rules: {
        userName: [{ required: true, message: this.$t("valid.name") }],
        password: [{ required: true, message: this.$t("valid.password") }],
        email: [{ required: true, message: this.$t("valid.email") }],
        roleNames: [{ required: true, message: this.$t("valid.roles") }],
        phoneNumber: [{ validator: validPhone, trigger: "blur" }],
      },
    };
  },
  created() {
    _this = this;
  },
  computed: {
    isCreat() {
      if (this.id === "0") return true;
      else return false;
    },
  },
  mounted() {
    this.role();
  },
  methods: {
    role: async function () {
      try {
        let roles = await GetRoles();
        this.roles = roles.items;
      } catch (err) {
        console.log(err);
        this.$message.error(this.$t("load.roleError"));
      }
    },
    load: async function (id) {
      let _this = this;
      _this.id = id;
      let requests = [GetUserRoles(_this.id), GetUser(_this.id)];
      let split = function (roles, user) {
        _this.form = Object.assign(_this.form, user);
        roles.items.forEach((role) => {
          _this.form.roleNames.push(role.name);
        });
      };
      let spin = this.$spin({ text: _this.$t("load.user") });
      try {
        let response = await axios.all(requests);
        split.apply(null, response);
      } catch (error) {
        this.$message.error(this.$t("load.userError"));
        this.visible = false;
      } finally {
        spin.close();
      }
    },
    close: function () {
      this.$emit("close");
    },
    save: async function () {
      let _this = this;
      let result = await this.valid();
      if (!result) return;
      let action;
      if (!_this.isCreat) action = Update(_this.id, _this.form);
      else action = Add(_this.form);
      try {
        _this.loading = true;
        await action;
        _this.$message.success(_this.$t("save.success"), () => {
          _this.$emit("reload");
        });
        this.close();
      } catch (err) {
        _this.$message.error(err.message);
      } finally {
        _this.loading = false;
      }
    },
    valid: async function () {
      return new Promise((resolve) => {
        this.$refs.userForm.validate((valid) => {
          resolve(valid);
        });
      });
    },
    change: function () {
      this.$refs.roleNames.blur();
    },
  },
};
</script>
