<template>
  <a-modal
    ref="dialog"
    :width="480"
    :title="$t('title.title')"
    :visible="true"
    class="default-hb-modal"
  >
    <a-form-model
      :model="form"
      ref="roleForm"
      layout="horizontal"
      :rules="rules"
      v-bind="formItemLayout"
    >
      <a-form-model-item :label="$t('title.name')" prop="name" class="hb-form-col-2">
        <a-input v-model="form.name" :placeholder="$t('placeholder.name')"></a-input>
      </a-form-model-item>
      <a-form-model-item
        :label="$t('title.description')"
        prop="description"
        class="hb-form-col-2"
      >
        <a-textarea
          v-model="form.description"
          :placeholder="$t('placeholder.description')"
        ></a-textarea>
        <!-- <a-input v-model="form.description" :placeholder="$t('placeholder.name')"> 
        </a-input>-->
      </a-form-model-item>
      <a-form-model-item :label="$t('title.remark')" prop="remark" class="hb-form-col-2">
        <a-textarea
          v-model="form.remark"
          :placeholder="$t('placeholder.remark')"
        ></a-textarea>
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
import { Add, Update, Get } from "@/api/role";
import locale from "@/localize/system/role";
export default {
  name: "RoleForm",
  i18n: {
    messages: locale,
  },
  data() {
    return {
      formItemLayout: {
        labelCol: { span: 4 },
        wrapperCol: { span: 14 },
      },
      loading: false,
      form: {
        name: "",
        description: "",
        remark: "",
        isDefault: true,
        isPublic: true,
      },
      rules: {
        name: [{ required: true, message: this.$t("valid.name") }],
      },
    };
  },
  methods: {
    save: async function () {
      let _this = this;
      let action;
      if (!(await this.valid())) return;
      if (_this.id) action = Update(_this.id, _this.form);
      else action = Add(_this.form);
      _this.loading = true;
      try {
        await action;
        _this.$message.success(_this.$t("save.success"));
        _this.close();
      } catch (err) {
      } finally {
        _this.loading = false;
      }
    },
    load: async function (id) {
      this.id = id;
      let spin = this.$spin({ text: this.$t("load.data") });
      try {
        let data = await Get(this.id);
        this.form = Object.assign(this.form, data);
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        spin.close();
      }
    },
    close: function () {
      this.$emit("close");
    },
    valid: function () {
      return new Promise((resolve) => {
        this.$refs.roleForm.validate((valid) => {
          resolve(valid);
        });
      });
    },
  },
};
</script>
<style lang="less" scoped>
/deep/.ant-form-item {
  margin-bottom: 4px !important;
}
</style>
