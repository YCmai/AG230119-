<template>
  <a-modal
    :visible="true"
    :width="620"
    :title="$t('title.form')"
    class="default-hb-modal"
  >
    <a-form-model
      :model="form"
      ref="wareForm"
      layout="inline"
      :rules="rules"
      class="ant-form-lable-100"
    >
      <a-form-model-item label="库别编码" prop="id">
        <a-input v-model="form.id"></a-input>
      </a-form-model-item>
      <a-form-model-item label="库别名称" prop="name">
        <a-input v-model="form.name"></a-input>
      </a-form-model-item>
      <a-form-model-item label="库别描述" prop="description" class="ant-form-item-100">
        <a-textarea v-model="form.description"></a-textarea>
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
import locale from "@/localize/warehouse/warehouse";
import component from "@/lib/base";
import {
  CreateWareHouse,
  UpdateWareHouse,
  GetWareHouse,
} from "@/api/inventory/wareHouse";
export default {
  name: "WareHouseForm",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  data() {
    return {
      loading: false,
      form: {
        id: "",
        name: "",
        description: "",
      },
      rules: {
        id: [{ required: true, message: "库别编码不能为空!", trigger: "blur" }],
        name: [{ required: true, message: "库别名称不能为空!", trigger: "blur" }],
      },
    };
  },
  methods: {
    load: async function (id) {
      this.loading = true;
      try {
        let record = await GetWareHouse(id);
        this.form = Object.assign(this.form, record);
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    save: async function () {
      if (!(await this.valid())) return;
      this.loading = true;
      try {
        if (this.form.concurrencyStamp)
          await UpdateWareHouse(this.form.id, {
            name: this.form.name,
            description: this.form.description,
            concurrencyStamp: this.form.concurrencyStamp,
          });
        else await CreateWareHouse(this.form);
        this.$message.success("保存数据成功!");
        this.close();
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    close: function () {
      this.$emit("close");
    },
    valid: function () {
      return new Promise((resolve) => {
        this.$refs.wareForm.validate((valid) => {
          resolve(valid);
        });
      });
    },
  },
};
</script>
